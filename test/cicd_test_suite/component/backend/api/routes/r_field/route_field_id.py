from utility import TestSuite, Assertion, CustomDate
import psycopg2
import os
import requests
from utility import JWTRegistry, UlidGenerator
import jose
from component.backend.api.utility.postgres import PostgresSuite
import datetime
import time
import json
from faker import Faker

__location__ = os.path.realpath(
    os.path.join(os.getcwd(), os.path.dirname(__file__)))

databaseConfig = {
    "host": None,
    "port": None,
    "user": None,
    "password": None,
    "database": None,
    "initial_date": None
}

backendConfig = {
    "host": None,
    "port": None,
    "iss": None,
    "aud": None
}

def getPostgresConnection():
    return psycopg2.connect(
        host=databaseConfig["host"],
        port=databaseConfig["port"],
        database=databaseConfig["database"],
        user=databaseConfig["user"],
        password=databaseConfig["password"]
    )


def test1(scope):
    scope.set_header('Test PATCH /field/:field_id should succeed and update the FarmField')

    fake = Faker('it_IT')
    Faker.seed(0)

    vat_number = fake.random_number(digits=11)
    id = UlidGenerator.generate()

    conn = getPostgresConnection()
    cur = conn.cursor()
    cur.execute('''
        insert into company(vat_number, industry_sector) values('{vat_number}', 'FAR');
    '''.format(vat_number=vat_number))
    cur.execute('''
        insert into company_far(vat_number, industry_sector) values('{vat_number}', 'FAR');
    '''.format(vat_number=vat_number))
    cur.execute('''
        insert into farm_field(id, vat_number) values
        ('{id}', '{vat_number}');
    '''.format(id=id, vat_number=vat_number))
    cur.execute('''
        insert into farm_field_versioning(field_id, vat_number, square_meters, crop_type, irrigation_type) values
        ('{id}', '{vat_number}', 5000, 'grain', 'drip');            
    '''.format(id=id, vat_number=vat_number))

    cur.close()
    conn.commit()
    conn.close()

    jwt_payload = {
        "company_vat_number": str(vat_number),
        "role": "FA",
        "aud": backendConfig["aud"],
        "iss": backendConfig["iss"],
        "sub": "test-user"
    }
    keys = JWTRegistry.plainMappedKeys()
    sign_key = keys[0]["key"]

    cDae = CustomDate.parse(backendConfig["initial_date"])
    utc_date = cDae.epoch()

    #sign jwt with custom iat time
    jwt_payload["iat"] = utc_date - 3600
    jwt_payload["exp"] = utc_date + 3600

    jwt = jose.jwt.encode(jwt_payload, sign_key, algorithm="RS256", headers={"kid": keys[0]["kid"]})
    print(jwt)


    response = requests.patch(
        f"http://{backendConfig['host']}:{backendConfig['port']}/field/{id}",
        timeout=2,
        headers={
            "Authorization": f"Bearer {jwt}"
        },
        json={
            "square_meters": 500,
            "crop_type": "vine",
            "irrigation_type": "sprinkler"
        }
    )
    Assertion.Equals(
        scope,
        "Should accept the request with a 200",
        201,
        response.status_code
    )


    conn = getPostgresConnection()
    cur = conn.cursor()
    cur.execute('''
        select farm_field.id, farm_field_versioning.square_meters, farm_field_versioning.crop_type, farm_field_versioning.irrigation_type
        from farm_field inner join farm_field_versioning on farm_field.id = farm_field_versioning.field_id
        where farm_field.vat_number = '{vat_number}';
    '''.format(vat_number=vat_number))
    result = cur.fetchall()
    cur.close()
    conn.commit()
    conn.close()

    Assertion.Equals(
        scope,
        "Should provide the expected list size",
        2,
        len(result)
    )





def TierUp():
    PostgresSuite.clearDatabase(getPostgresConnection())

def TierDown():
    PostgresSuite.clearDatabase(getPostgresConnection())

def ClearDatabase():
    PostgresSuite.clearDatabase(getPostgresConnection())
    

def EntryPoint(
    *args,
    **kwargs
):
    databaseConfig["host"] = args[0]
    databaseConfig["port"] = args[1]
    databaseConfig["database"] = args[2]
    databaseConfig["user"] = args[3]
    databaseConfig["password"] = args[4]
    backendConfig["host"] = args[5]
    backendConfig["port"] = args[6]
    backendConfig["iss"] = args[7]
    backendConfig["aud"] = args[8]
    backendConfig["initial_date"] = args[9]


    suite = TestSuite()
    suite.set_tierup(TierUp)
    suite.set_tierdown(TierDown)
    suite.set_middletier(ClearDatabase)
    suite.add_assertion(test1)
    suite.run()
    suite.print_stats()