from utility import TestSuite, Assertion, CustomDate, Encode
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
    scope.set_header('Test GET /resourcemanager/field should succeed and return a list of fields')

    fake = Faker('it_IT')
    Faker.seed(0)

    vat_number = fake.random_number(digits=11)
    field_ids = [UlidGenerator.generate() for _ in range(5)]
    company_secret = "yXab93xC92s4sGjH"

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
        ('{ids[0]}', '{vat_number}'),
        ('{ids[1]}', '{vat_number}'),
        ('{ids[2]}', '{vat_number}'),
        ('{ids[3]}', '{vat_number}'),
        ('{ids[4]}', '{vat_number}');
    '''.format(ids=field_ids, vat_number=vat_number))
    cur.execute('''
        insert into farm_field_versioning(field_id, vat_number, square_meters, crop_type, irrigation_type) values
        ('{ids[0]}', '{vat_number}', 1000, 'wheat', 'drip'),
        ('{ids[1]}', '{vat_number}', 2000, 'corn', 'drip'),
        ('{ids[2]}', '{vat_number}', 3000, 'rice', 'drip'),
        ('{ids[3]}', '{vat_number}', 4000, 'barley', 'drip'),
        ('{ids[4]}', '{vat_number}', 5000, 'grain', 'drip');            
    '''.format(ids=field_ids, vat_number=vat_number))
    cur.execute('''
        insert into secret_key(company_vat_number, secret_key) values('{vat_number}', '{company_secret}');
    '''.format(vat_number=vat_number, company_secret=company_secret))

    cur.close()
    conn.commit()
    conn.close()

    json_payload = {
        "sub": str(vat_number),
        "method": "GET",
        "path": "/resourcemanager/field"
    }

    cDae = CustomDate.parse(backendConfig["initial_date"])
    utc_date = cDae.epoch()

    #sign jwt with custom iat time
    json_payload["iat"] = utc_date - 3600
    json_payload["exp"] = utc_date + 3600

    json_string = json.dumps(json_payload)
    encoded_payload = Encode.base64url_encode(json_string)
    signature = Encode.hmac_sha256_encode(company_secret, encoded_payload)
    encoded_signature = Encode.base64url_encode(signature)



    response = requests.get(
        f"http://{backendConfig['host']}:{backendConfig['port']}/resourcemanager/field",
        timeout=2,
        headers={
            "Authorization": f"Pissir-farm-hmac-sha256 {encoded_payload}.{encoded_signature}"
        }
    )
    Assertion.Equals(
        scope,
        "Should accept the request with a 200",
        200,
        response.status_code
    )
    Assertion.Equals(
        scope,
        "Should provide the expected list size",
        5,
        len(response.json())
    )



def test2(scope):
    scope.set_header('Test GET /resourcemanager/field should succeed and return a list of fields')

    fake = Faker('it_IT')
    Faker.seed(0)

    vat_number = fake.random_number(digits=11)
    field_ids = [UlidGenerator.generate() for _ in range(2)]
    company_secret = "yXab93xC92s4sGjH"
    date_minus1 = CustomDate.parse(backendConfig["initial_date"]).addDays(-1).toISODate()
    date_minus2 = CustomDate.parse(backendConfig["initial_date"]).addDays(-2).toISODate()
    date_minus3 = CustomDate.parse(backendConfig["initial_date"]).addDays(-3).toISODate()

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
        ('{ids[0]}', '{vat_number}'),
        ('{ids[1]}', '{vat_number}');
    '''.format(ids=field_ids, vat_number=vat_number))
    cur.execute('''
        insert into farm_field_versioning(field_id, vat_number, square_meters, crop_type, irrigation_type, created_at) values
        ('{ids[0]}', '{vat_number}', 1000, 'wheat', 'drip', '{date_minus3}'),
        ('{ids[0]}', '{vat_number}', 2000, 'corn', 'drip', '{date_minus2}'),
        ('{ids[0]}', '{vat_number}', 3000, 'rice', 'drip', '{date_minus1}'),
        ('{ids[1]}', '{vat_number}', 4000, 'barley', 'drip', '{date_minus2}'),
        ('{ids[1]}', '{vat_number}', 5000, 'grain', 'flood', '{date_minus1}');
    '''.format(ids=field_ids, vat_number=vat_number, date_minus1=date_minus1, date_minus2=date_minus2, date_minus3=date_minus3))
    cur.execute('''
        insert into secret_key(company_vat_number, secret_key) values('{vat_number}', '{company_secret}');
    '''.format(vat_number=vat_number, company_secret=company_secret))

    cur.close()
    conn.commit()
    conn.close()

    json_payload = {
        "sub": str(vat_number),
        "method": "GET",
        "path": "/resourcemanager/field"
    }

    cDae = CustomDate.parse(backendConfig["initial_date"])
    utc_date = cDae.epoch()

    #sign jwt with custom iat time
    json_payload["iat"] = utc_date - 3600
    json_payload["exp"] = utc_date + 3600

    json_string = json.dumps(json_payload)
    encoded_payload = Encode.base64url_encode(json_string)
    signature = Encode.hmac_sha256_encode(company_secret, encoded_payload)
    encoded_signature = Encode.base64url_encode(signature)



    response = requests.get(
        f"http://{backendConfig['host']}:{backendConfig['port']}/resourcemanager/field",
        timeout=2,
        headers={
            "Authorization": f"Pissir-farm-hmac-sha256 {encoded_payload}.{encoded_signature}"
        }
    )
    Assertion.Equals(
        scope,
        "Should accept the request with a 200",
        200,
        response.status_code
    )
    Assertion.Equals(
        scope,
        "Should provide the expected list size",
        2,
        len(response.json())
    )

    for field in response.json():
        if field["id"] == field_ids[0]:
            Assertion.Equals(
                scope,
                "Should provide the expected square meters",
                3000,
                field["square_meters"]
            )
            Assertion.Equals(
                scope,
                "Should provide the expected crop type",
                "rice",
                field["crop_type"]
            )
            Assertion.Equals(
                scope,
                "Should provide the expected irrigation type",
                "drip",
                field["irrigation_type"]
            )
        if field["id"] == field_ids[1]:
            Assertion.Equals(
                scope,
                "Should provide the expected square meters",
                5000,
                field["square_meters"]
            )
            Assertion.Equals(
                scope,
                "Should provide the expected crop type",
                "grain",
                field["crop_type"]
            )
            Assertion.Equals(
                scope,
                "Should provide the expected irrigation type",
                "flood",
                field["irrigation_type"]
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
    suite.add_assertion(test2)
    suite.run()
    suite.print_stats()