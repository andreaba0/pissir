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
    scope.set_header('Test GET /water/consumption should return a list of water consumption for a given company')

    fake = Faker('it_IT')
    Faker.seed(0)

    vat_number_far = fake.random_number(digits=11)
    vat_number_far2 = fake.random_number(digits=11)
    vat_number_wsp = fake.random_number(digits=11)
    company_chosen_id = fake.random_number(digits=11)
    offer_ids = [UlidGenerator.generate() for _ in range(5)]
    field_ids = [UlidGenerator.generate() for _ in range(2)]
    fields_ids_company2 = [UlidGenerator.generate() for _ in range(2)]
    actuator_id = UlidGenerator.generate()
    tomorrow = CustomDate.parse(backendConfig["initial_date"]).addDays(1).toISODate()
    tomorrow_date = CustomDate.parse(backendConfig["initial_date"]).addDays(1)

    conn = getPostgresConnection()
    cur = conn.cursor()
    cur.execute('''
        insert into company(vat_number, industry_sector) values
        ('{vat_number_far}', 'FAR'),
        ('{vat_number_wsp}', 'WSP'),
        ('{vat_number_far2}', 'FAR');
    '''.format(vat_number_far=vat_number_far, vat_number_wsp=vat_number_wsp, vat_number_far2=vat_number_far2))
    cur.execute('''
        insert into company_wsp(vat_number, industry_sector) values('{vat_number_wsp}', 'WSP');
    '''.format(vat_number_wsp=vat_number_wsp))
    cur.execute('''
        insert into company_far(vat_number, industry_sector) values('{vat_number_far}', 'FAR'), ('{vat_number_far2}', 'FAR');
    '''.format(vat_number_far=vat_number_far, vat_number_far2=vat_number_far2))
    cur.execute('''
        insert into offer(id, vat_number, publish_date, price_liter, available_liters, purchased_liters) values
        ('{ids[0]}', '{vat_number_wsp}', '{tomorrow}', 5.0, 100000, 0),
        ('{ids[1]}', '{vat_number_wsp}', '{tomorrow}', 3.0, 70000, 0),
        ('{ids[2]}', '{vat_number_wsp}', '{tomorrow}', 4.0, 30000, 0),
        ('{ids[3]}', '{vat_number_wsp}', '{tomorrow}', 1.0, 30000, 0),
        ('{ids[4]}', '{vat_number_wsp}', '{tomorrow}', 2.0, 80000, 0);
    '''.format(ids=offer_ids, vat_number_far=vat_number_far, vat_number_wsp=vat_number_wsp, tomorrow=tomorrow))
    cur.execute('''
        insert into farm_field(id, vat_number) values
        ('{ids[0]}', '{vat_number}'),
        ('{ids[1]}', '{vat_number}'),
        ('{ids2[0]}', '{vat_number2}'),
        ('{ids2[1]}', '{vat_number2}');       
    '''.format(ids=field_ids, vat_number=vat_number_far, ids2=fields_ids_company2, vat_number2=vat_number_far2))
    cur.execute('''
        insert into buy_order(offer_id, farm_field_id, qty) values
        ('{ids[0]}', '{field_ids[0]}', 6000),
        ('{ids[1]}', '{field_ids[0]}', 200),
        ('{ids[1]}', '{field_ids[1]}', 300);
    '''.format(ids=offer_ids, field_ids=field_ids))
    cur.execute('''
        insert into object_logger(id, company_chosen_id, object_type, farm_field_id) values
        ('{id}', '{company_chosen_id}', 'ACTUATOR', '{field_id}');
    '''.format(
            id=actuator_id,
            company_chosen_id=company_chosen_id,
            field_id=field_ids[0]
    ))
    cur.execute('''
        insert into actuator_log(object_id, log_time, is_active, active_time, water_used, object_type) values
        ('{id}', '{day1time1}', true, 100, 1000, 'ACTUATOR'),
        ('{id}', '{day1time2}', true, 100, 2000, 'ACTUATOR');
    ''' .format(
            id=actuator_id,
            day1time1=tomorrow_date.addHours(3).toISODate(),
            day1time2=tomorrow_date.addHours(8).toISODate()
    ))

    cur.close()
    conn.commit()
    conn.close()

    jwt_payload = {
        "company_vat_number": str(vat_number_far),
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
        f"http://{backendConfig['host']}:{backendConfig['port']}/water/consumption",
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

    for item in response.json():
        if item["field_id"] == field_ids[0]:
            Assertion.Equals(
                scope,
                "Should provide the expected water used",
                3000,
                item["amount_used"]
            )
            Assertion.Equals(
                scope,
                "Should provide the expected amount of water ordered",
                6200,
                item["amount_ordered"]
            )
        elif item["field_id"] == field_ids[1]:
            Assertion.Equals(
                scope,
                "Should provide the expected water used",
                0,
                item["amount_used"]
            )
            Assertion.Equals(
                scope,
                "Should provide the expected amount of water ordered",
                300,
                item["amount_ordered"]
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