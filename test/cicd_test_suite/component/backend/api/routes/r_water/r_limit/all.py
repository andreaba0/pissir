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
    scope.set_header('Test GET /water/limit/all should succeed and return the expected list')

    fake = Faker('it_IT')
    Faker.seed(0)

    vat_number = fake.random_number(digits=11)
    vat_number2 = fake.random_number(digits=11)
    today = CustomDate.parse(backendConfig["initial_date"]).toISODate()
    tomorrow = CustomDate.parse(backendConfig["initial_date"]).addDays(1).toISODate()
    day_after_tomorrow = CustomDate.parse(backendConfig["initial_date"]).addDays(2).toISODate()
    print(today)
    print(tomorrow)
    print(day_after_tomorrow)

    conn = getPostgresConnection()
    cur = conn.cursor()
    cur.execute('''
        insert into company(vat_number, industry_sector) values('{vat_number}', 'FAR'), ('{vat_number2}', 'FAR');
    '''.format(vat_number=vat_number, vat_number2=vat_number2))
    cur.execute('''
        insert into company_far(vat_number, industry_sector) values('{vat_number}', 'FAR'), ('{vat_number2}', 'FAR');
    '''.format(vat_number=vat_number, vat_number2=vat_number2))
    cur.execute('''
        insert into daily_water_limit(vat_number, consumption_sign, available, consumed, on_date) values
        ('{vat_number}', 1, 1000, 0, '{date}'),
        ('{vat_number}', 1, 1000, 0, '{date1}'),
        ('{vat_number}', 1, 1000, 0, '{date2}'),
        ('{vat_number2}', 1, 900, 100, '{date}'),
        ('{vat_number2}', 1, 1000, 0, '{date1}');
    '''.format(
        vat_number=vat_number,
        vat_number2=vat_number2,
        date=today,
        date1=tomorrow,
        date2=day_after_tomorrow
    ))

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
        f"http://{backendConfig['host']}:{backendConfig['port']}/water/limit/all",
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
    print(response.json())
    Assertion.Equals(
        scope,
        "Should provide the expected list size",
        2,
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