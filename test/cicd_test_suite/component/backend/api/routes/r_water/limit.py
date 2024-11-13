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
    scope.set_header('Test GET /water/limit should succeed and return a float')

    fake = Faker('it_IT')
    Faker.seed(0)

    vat_number1 = fake.random_number(digits=11)
    vat_number2 = fake.random_number(digits=11)
    vat_number_wsp = fake.random_number(digits=11)
    offer_ids = [UlidGenerator.generate() for _ in range(2)]
    farm_field_ids = [UlidGenerator.generate() for _ in range(2)]

    conn = getPostgresConnection()
    cur = conn.cursor()
    cur.execute('''
        insert into company(vat_number, industry_sector) values
            ('{vat_number1}', 'FAR'),
            ('{vat_number2}', 'FAR'),
            ('{vat_number_wsp}', 'WSP');
    '''.format(vat_number1=vat_number1, vat_number2=vat_number2, vat_number_wsp=vat_number_wsp))
    cur.execute('''
        insert into company_far(vat_number, industry_sector) values('{vat_number1}', 'FAR'), ('{vat_number2}', 'FAR');
    '''.format(vat_number1=vat_number1, vat_number2=vat_number2))
    cur.execute('''
        insert into company_wsp(vat_number, industry_sector) values('{vat_number_wsp}', 'WSP');
    '''.format(vat_number_wsp=vat_number_wsp))
    cur.execute('''
        insert into farm_field(id, vat_number) values
        ('{ids[0]}', '{vat_number1}'),
        ('{ids[1]}', '{vat_number2}');
    '''.format(ids=farm_field_ids, vat_number1=vat_number1, vat_number2=vat_number2))
    cur.execute('''
        insert into offer(id, vat_number, publish_date, price_liter, available_liters, purchased_liters) values
        ('{offer_ids[0]}', '{vat_number_wsp}', '{day1}', 1.0, 1000, 600);
    '''.format(
        offer_ids=offer_ids,
        vat_number_wsp=vat_number_wsp,
        day1=CustomDate.parse(backendConfig["initial_date"]).toISODate()
    ))
    cur.execute('''
        insert into buy_order(offer_id, farm_field_id, qty) values
        ('{offer_id}', '{field_id}', 100);
    '''.format(offer_id=offer_ids[0], field_id=farm_field_ids[0]))
    cur.execute('''
        insert into daily_water_limit(vat_number, consumption_sign, available, consumed, on_date) values
        ('{vat_number2}', 1, 1000, 0, '{date}');
    '''.format(vat_number2=vat_number2, date=CustomDate.parse(backendConfig["initial_date"]).toISODate()))

    cur.close()
    conn.commit()
    conn.close()

    jwt_payload = {
        "company_vat_number": str(vat_number1),
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
        f"http://{backendConfig['host']}:{backendConfig['port']}/water/limit",
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
        "Should return the correct value",
        str(100),
        response.text
    )



    jwt_payload = {
        "company_vat_number": str(vat_number2),
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
        f"http://{backendConfig['host']}:{backendConfig['port']}/water/limit",
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
        "Should return the correct value",
        str(1000),
        response.text
    )




def test2(scope):
    scope.set_header('Test GET /water/limit should return 0 if no limit is set')

    fake = Faker('it_IT')
    Faker.seed(0)

    vat_number = fake.random_number(digits=11)

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
        f"http://{backendConfig['host']}:{backendConfig['port']}/water/limit",
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
        "Should return the correct value",
        str(0),
        response.text
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