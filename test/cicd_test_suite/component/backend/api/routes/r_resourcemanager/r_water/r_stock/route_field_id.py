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
    scope.set_header('Test GET /resourcemanager/water/stock/:field_id should succeed and return water stock for the field')

    fake = Faker('it_IT')
    Faker.seed(0)

    vat_number = fake.random_number(digits=11)
    vat_number_wsp = fake.random_number(digits=11)
    offer_ids = [UlidGenerator.generate() for _ in range(2)]
    field_id = UlidGenerator.generate()
    company_secret = "yXab93xC92s4sGjH"
    date_minus1 = CustomDate.parse(backendConfig["initial_date"]).addDays(-1).toISODate()
    date_minus2 = CustomDate.parse(backendConfig["initial_date"]).addDays(-2).toISODate()
    date_minus3 = CustomDate.parse(backendConfig["initial_date"]).addDays(-3).toISODate()
    today = CustomDate.parse(backendConfig["initial_date"]).toISODate()

    conn = getPostgresConnection()
    cur = conn.cursor()
    cur.execute('''
        insert into company(vat_number, industry_sector) values('{vat_number}', 'FAR'), ('{vat_number_wsp}', 'WSP');
    '''.format(vat_number=vat_number, vat_number_wsp=vat_number_wsp))
    cur.execute('''
        insert into company_far(vat_number, industry_sector) values('{vat_number}', 'FAR');
    '''.format(vat_number=vat_number))
    cur.execute('''
        insert into company_wsp(vat_number, industry_sector) values('{vat_number}', 'WSP');
    '''.format(vat_number=vat_number_wsp))
    cur.execute('''
        insert into farm_field(id, vat_number) values
        ('{field_id}', '{vat_number}');
    '''.format(field_id=field_id, vat_number=vat_number))
    cur.execute('''
        insert into farm_field_versioning(field_id, vat_number, square_meters, crop_type, irrigation_type, created_at) values
        ('{id}', '{vat_number}', 1000, 'wheat', 'drip', '{date_minus3}'),
        ('{id}', '{vat_number}', 2000, 'corn', 'flood', '{date_minus2}'),
        ('{id}', '{vat_number}', 3000, 'rice', 'subsurface', '{date_minus1}'); 
    '''.format(id=field_id, vat_number=vat_number, date_minus1=date_minus1, date_minus2=date_minus2, date_minus3=date_minus3))
    cur.execute('''
        insert into offer(id, vat_number, publish_date, price_liter, available_liters, purchased_liters) values
        ('{ids[0]}', '{wa_vat_number}', '{today}', 0.5, 1000, 100),
        ('{ids[1]}', '{wa_vat_number}', '{today}', 0.6, 2000, 200);
    '''.format(ids=offer_ids, wa_vat_number=vat_number_wsp, today=today))
    cur.execute('''
        insert into buy_order(offer_id, farm_field_id, qty) values
        ('{offer_ids[0]}', '{field_id}', 100),
        ('{offer_ids[1]}', '{field_id}', 200);
    '''.format(offer_ids=offer_ids, field_id=field_id))
    cur.execute('''
        insert into secret_key(company_vat_number, secret_key) values('{vat_number}', '{company_secret}');
    '''.format(vat_number=vat_number, company_secret=company_secret))

    cur.close()
    conn.commit()
    conn.close()

    json_payload = {
        "sub": str(vat_number),
        "method": "GET",
        "path": "/resourcemanager/water/stock/" + field_id
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
        f"http://{backendConfig['host']}:{backendConfig['port']}/resourcemanager/water/stock/{field_id}",
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
        "Should return the value of the correct field",
        field_id,
        response.json()["field_id"]
    )
    Assertion.Equals(
        scope,
        "Should return the correct limit value",
        300,
        response.json()["limit"]
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