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
    scope.set_header('Test GET /water/order should return a list of orders of same WA company')

    fake = Faker('it_IT')
    Faker.seed(0)

    vat_number = fake.random_number(digits=11)
    vat_number2 = fake.random_number(digits=11)
    vat_number3 = fake.random_number(digits=11)
    ids = [UlidGenerator.generate() for _ in range(5)]
    field_ids = [UlidGenerator.generate() for _ in range(5)]
    tomorrow = CustomDate.parse(backendConfig["initial_date"]).addDays(1).toISODate()

    conn = getPostgresConnection()
    cur = conn.cursor()
    cur.execute('''
        insert into company(vat_number, industry_sector) values
        ('{vat_number}', 'FAR'),
        ('{vat_number2}', 'WSP'),
        ('{vat_number3}', 'FAR');
    '''.format(vat_number=vat_number, vat_number2=vat_number2, vat_number3=vat_number3))
    cur.execute('''
        insert into company_wsp(vat_number, industry_sector) values('{vat_number2}', 'WSP');
    '''.format(vat_number2=vat_number2))
    cur.execute('''
        insert into company_far(vat_number, industry_sector) values('{vat_number}', 'FAR'), ('{vat_number3}', 'FAR');
    '''.format(vat_number=vat_number, vat_number3=vat_number3))
    cur.execute('''
        insert into offer(id, vat_number, publish_date, price_liter, available_liters, purchased_liters) values
        ('{ids[0]}', '{vat_number2}', '{day1}', 1.0, 1000, 100),
        ('{ids[1]}', '{vat_number2}', '{day2}', 3.0, 1000, 200),
        ('{ids[2]}', '{vat_number2}', '{day3}', 4.0, 1000, 300),
        ('{ids[3]}', '{vat_number2}', '{day4}', 1.0, 1000, 400),
        ('{ids[4]}', '{vat_number2}', '{day5}', 2.0, 1000, 500);
    '''.format(
            ids=ids, 
            vat_number=vat_number, 
            vat_number2=vat_number2,
            day1=CustomDate.parse(backendConfig["initial_date"]).addDays(1).toISODate(),
            day2=CustomDate.parse(backendConfig["initial_date"]).addDays(2).toISODate(),
            day3=CustomDate.parse(backendConfig["initial_date"]).addDays(3).toISODate(),
            day4=CustomDate.parse(backendConfig["initial_date"]).addDays(4).toISODate(),
            day5=CustomDate.parse(backendConfig["initial_date"]).addDays(5).toISODate()
    ))
    cur.execute('''
        insert into farm_field(id, vat_number) values
        ('{field_ids[0]}', '{vat_number}'),
        ('{field_ids[1]}', '{vat_number}'),
        ('{field_ids[2]}', '{vat_number3}'),
        ('{field_ids[3]}', '{vat_number3}'),
        ('{field_ids[4]}', '{vat_number3}');
    '''.format(field_ids=field_ids, vat_number=vat_number, vat_number2=vat_number2, vat_number3=vat_number3))
    cur.execute('''
        insert into buy_order(offer_id, farm_field_id, qty) values
        ('{ids[0]}', '{field_ids[0]}', 100),
        ('{ids[1]}', '{field_ids[1]}', 200),
        ('{ids[2]}', '{field_ids[2]}', 300),
        ('{ids[3]}', '{field_ids[3]}', 400),
        ('{ids[4]}', '{field_ids[4]}', 500);
    '''.format(ids=ids, field_ids=field_ids))
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


    response = requests.get(
        f"http://{backendConfig['host']}:{backendConfig['port']}/water/order",
        timeout=2,
        headers={
            "Authorization": f"Bearer {jwt}"
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



    jwt_payload = {
        "company_vat_number": str(vat_number2),
        "role": "WA",
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


    response = requests.get(
        f"http://{backendConfig['host']}:{backendConfig['port']}/water/order",
        timeout=2,
        headers={
            "Authorization": f"Bearer {jwt}"
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