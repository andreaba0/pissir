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
    scope.set_header('Test GET /water/stock should reject with expired token')

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
        f"http://{backendConfig['host']}:{backendConfig['port']}/water/stock",
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
    scope.set_header('Test GET /water/stock should succeed and return json[WaterStock]')

    fake = Faker('it_IT')
    Faker.seed(0)

    vat_number = fake.random_number(digits=11)
    ids = [UlidGenerator.generate() for _ in range(2)]
    offer_ids = [UlidGenerator.generate() for _ in range(2)]
    wa_vat_number = fake.random_number(digits=11)
    today = CustomDate.parse(backendConfig["initial_date"]).toISODate()

    conn = getPostgresConnection()
    cur = conn.cursor()
    cur.execute('''
        insert into company(vat_number, industry_sector) values('{vat_number}', 'FAR');
    '''.format(vat_number=vat_number))
    cur.execute('''
        insert into company_far(vat_number, industry_sector) values('{vat_number}', 'FAR');
    '''.format(vat_number=vat_number))
    cur.execute('''
        insert into company(vat_number, industry_sector) values('{wa_vat_number}', 'WSP');
    '''.format(wa_vat_number=wa_vat_number))
    cur.execute('''
        insert into company_wsp(vat_number, industry_sector) values('{wa_vat_number}', 'WSP');
    '''.format(wa_vat_number=wa_vat_number))
    cur.execute('''
        insert into farm_field(id, vat_number) values
        ('{ids[0]}', '{vat_number}'),
        ('{ids[1]}', '{vat_number}');
    '''.format(ids=ids, vat_number=vat_number))
    cur.execute('''
        insert into farm_field_versioning(field_id, vat_number, square_meters, crop_type, irrigation_type) values
        ('{ids[0]}', '{vat_number}', 1000, 'wheat', 'drip'),
        ('{ids[1]}', '{vat_number}', 2000, 'corn', 'drip');           
    '''.format(ids=ids, vat_number=vat_number))
    cur.execute('''
        insert into offer(id, vat_number, publish_date, price_liter, available_liters, purchased_liters) values
        ('{ids[0]}', '{wa_vat_number}', '{today}', 0.5, 1000, 100),
        ('{ids[1]}', '{wa_vat_number}', '{today}', 0.6, 2000, 200);
    '''.format(ids=offer_ids, wa_vat_number=wa_vat_number, today=today))
    cur.execute('''
        insert into buy_order(offer_id, farm_field_id, qty) values
        ('{offer_ids[0]}', '{ids[0]}', 100),
        ('{offer_ids[1]}', '{ids[1]}', 200);
    '''.format(offer_ids=offer_ids, ids=ids))

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
        f"http://{backendConfig['host']}:{backendConfig['port']}/field",
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
        "Should return an array of 2 elements",
        2,
        len(response.json())
    )


    response = requests.get(
        f"http://{backendConfig['host']}:{backendConfig['port']}/field",
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
        "Should return an array of 2 elements",
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
    suite.add_assertion(test2)
    suite.run()
    suite.print_stats()