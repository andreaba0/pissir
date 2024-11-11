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
    scope.set_header('Test GET /field should reject with expired token')

    jwt_payload = {
        "company_vat_number": "test",
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
    jwt_payload["iat"] = utc_date - 3600 - 3600
    jwt_payload["exp"] = utc_date - 3600

    jwt = jose.jwt.encode(jwt_payload, sign_key, algorithm="RS256", headers={"kid": keys[0]["kid"]})
    
    response = requests.get(
        f"http://{backendConfig['host']}:{backendConfig['port']}/field",
        timeout=2,
        headers={
            "Authorization": f"Bearer {jwt}"
        }
    )
    Assertion.Equals(
        scope,
        "Should reject the request with a 401",
        401,
        response.status_code
    )
    Assertion.Equals(
        scope,
        "Should provide the expected message",
        "Token expired",
        response.text
    )


def test2(scope):
    scope.set_header('Test GET /water/offer should return a list of offers of same WA company')

    fake = Faker('it_IT')
    Faker.seed(0)

    vat_number = fake.random_number(digits=11)
    vat_number2 = fake.random_number(digits=11)
    ids = [UlidGenerator.generate() for _ in range(5)]
    tomorrow = CustomDate.parse(backendConfig["initial_date"]).addDays(1).toISODate()
    print(tomorrow)

    conn = getPostgresConnection()
    cur = conn.cursor()
    cur.execute('''
        insert into company(vat_number, industry_sector) values
        ('{vat_number}', 'WSP'),
        ('{vat_number2}', 'WSP');
    '''.format(vat_number=vat_number, vat_number2=vat_number2))
    cur.execute('''
        insert into company_wsp(vat_number, industry_sector) values('{vat_number}', 'WSP'), ('{vat_number2}', 'WSP');
    '''.format(vat_number=vat_number, vat_number2=vat_number2))
    cur.execute('''
        insert into offer(id, vat_number, publish_date, price_liter, available_liters, purchased_liters) values
        ('{ids[0]}', '{vat_number}', '{tomorrow}', 1.0, 1000, 0),
        ('{ids[1]}', '{vat_number}', '{tomorrow}', 3.0, 1000, 0),
        ('{ids[2]}', '{vat_number}', '{tomorrow}', 4.0, 1000, 0),
        ('{ids[3]}', '{vat_number2}', '{tomorrow}', 1.0, 1000, 0),
        ('{ids[4]}', '{vat_number2}', '{tomorrow}', 2.0, 1000, 0);
    '''.format(ids=ids, vat_number=vat_number, vat_number2=vat_number2, tomorrow=tomorrow))
    cur.close()
    conn.commit()
    conn.close()

    jwt_payload = {
        "company_vat_number": str(vat_number),
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
    print(jwt)


    response = requests.get(
        f"http://{backendConfig['host']}:{backendConfig['port']}/water/offer",
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
        3,
        len(response.json())
    )


def test3(scope):
    scope.set_header('Test GET /water/offer should return a list of all offers from all WA companies')

    fake = Faker('it_IT')
    Faker.seed(0)

    vat_number = fake.random_number(digits=11)
    vat_number2 = fake.random_number(digits=11)
    ids = [UlidGenerator.generate() for _ in range(5)]
    tomorrow = CustomDate.parse(backendConfig["initial_date"]).addDays(1).toISODate()
    print(tomorrow)

    conn = getPostgresConnection()
    cur = conn.cursor()
    cur.execute('''
        insert into company(vat_number, industry_sector) values
        ('{vat_number}', 'WSP'),
        ('{vat_number2}', 'WSP');
    '''.format(vat_number=vat_number, vat_number2=vat_number2))
    cur.execute('''
        insert into company_wsp(vat_number, industry_sector) values('{vat_number}', 'WSP'), ('{vat_number2}', 'WSP');
    '''.format(vat_number=vat_number, vat_number2=vat_number2))
    cur.execute('''
        insert into offer(id, vat_number, publish_date, price_liter, available_liters, purchased_liters) values
        ('{ids[0]}', '{vat_number}', '{tomorrow}', 1.0, 1000, 0),
        ('{ids[1]}', '{vat_number}', '{tomorrow}', 3.0, 1000, 0),
        ('{ids[2]}', '{vat_number}', '{tomorrow}', 4.0, 1000, 0),
        ('{ids[3]}', '{vat_number2}', '{tomorrow}', 1.0, 1000, 0),
        ('{ids[4]}', '{vat_number2}', '{tomorrow}', 2.0, 1000, 0);
    '''.format(ids=ids, vat_number=vat_number, vat_number2=vat_number2, tomorrow=tomorrow))
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


    response = requests.get(
        f"http://{backendConfig['host']}:{backendConfig['port']}/water/offer",
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
    suite.add_assertion(test2)
    suite.add_assertion(test3)
    suite.run()
    suite.print_stats()