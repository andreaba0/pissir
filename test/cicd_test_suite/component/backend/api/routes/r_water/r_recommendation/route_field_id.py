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
    vat_number_wsp = fake.random_number(digits=11)
    company_chosen_id = fake.random_number(digits=11)
    ids = [UlidGenerator.generate() for _ in range(2)]
    field_ids = [UlidGenerator.generate() for _ in range(2)]
    tomorrow = CustomDate.parse(backendConfig["initial_date"]).addDays(1).toISODate()
    print(tomorrow)

    conn = getPostgresConnection()
    cur = conn.cursor()
    cur.execute('''
        insert into company(vat_number, industry_sector) values
        ('{vat_number}', 'FAR'),
        ('{vat_number_wsp}', 'WSP');
    '''.format(vat_number=vat_number, vat_number_wsp=vat_number_wsp))
    cur.execute('''
        insert into company_far(vat_number, industry_sector) values('{vat_number}', 'FAR');
    '''.format(vat_number=vat_number))
    cur.execute('''
        insert into company_wsp(vat_number, industry_sector) values('{vat_number}', 'WSP');
    '''.format(vat_number=vat_number_wsp))
    cur.execute('''
        insert into offer(id, vat_number, publish_date, price_liter, available_liters, purchased_liters) values
        ('{ids[0]}', '{vat_number}', '{day1}', 1.0, 1000, 600);
    '''.format(
            ids=ids, 
            vat_number=vat_number_wsp,
            day1=CustomDate.parse(backendConfig["initial_date"]).toISODate()
    ))
    cur.execute('''
        insert into farm_field(id, vat_number) values
        ('{field_ids[0]}', '{vat_number}'),
        ('{field_ids[1]}', '{vat_number}');
    '''.format(field_ids=field_ids, vat_number=vat_number))
    cur.execute('''
        insert into farm_field_versioning(field_id, vat_number, square_meters, crop_type, irrigation_type, created_at) values
        ('{field_ids[0]}', '{vat_number}', 1000, 'wheat', 'drip', '{day1}'),
        ('{field_ids[0]}', '{vat_number}', 2000, 'corn', 'drip', '{day2}');
    ''' .format(
            field_ids=field_ids,
            vat_number=vat_number,
            day1=CustomDate.parse(backendConfig["initial_date"]).addDays(-2).toISODate(),
            day2=CustomDate.parse(backendConfig["initial_date"]).addDays(-1).toISODate()
    ))
    cur.execute('''
        insert into buy_order(offer_id, farm_field_id, qty) values
        ('{ids[0]}', '{field_ids[0]}', 600);
    ''' .format(ids=ids, field_ids=field_ids))
    cur.execute('''
        insert into object_logger(id, company_chosen_id, object_type, farm_field_id) values
        ('{ids[0]}', '{company_chosen_id}', 'ACTUATOR', '{field_ids[0]}');
    '''.format(
            ids=ids,
            company_chosen_id=company_chosen_id,
            field_ids=field_ids
    ))
    cur.execute('''
        insert into actuator_log(object_id, log_time, is_active, active_time, water_used, object_type) values
        ('{ids[0]}', '{day1time1}', true, 100, 100, 'ACTUATOR'),
        ('{ids[0]}', '{day1time2}', true, 100, 200, 'ACTUATOR');
    ''' .format(
            ids=ids,
            vat_number=vat_number,
            field_ids=field_ids,
            day1time1='2010-01-01T01:00:30',
            day1time2='2010-01-01T02:00:30'
    ))
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
        f"http://{backendConfig['host']}:{backendConfig['port']}/water/recommendation/{field_ids[0]}",
        timeout=2,
        headers={
            "Authorization": f"Bearer {jwt}"
        }
    )
    print(response.text)
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
    obj = response.json()
    total_estimated = 1800*2000
    total_remaining = 600-300 # water bought - water used
    Assertion.Equals(
        scope,
        "Should provide the expected total estimated",
        total_estimated,
        obj["total_estimated"]
    )
    Assertion.Equals(
        scope,
        "Should provide the expected total remaining",
        total_remaining,
        obj["total_remaining"]
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