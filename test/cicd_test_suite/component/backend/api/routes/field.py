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
    scope.set_header('Test GET /field should succeed and return json[FarmField]')

    fake = Faker('it_IT')
    Faker.seed(0)

    vat_number = fake.random_number(digits=11)
    ids = [UlidGenerator.generate() for _ in range(5)]

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
    '''.format(ids=ids, vat_number=vat_number))
    cur.execute('''
        insert into farm_field_versioning(field_id, vat_number, square_meters, crop_type, irrigation_type) values
        ('{ids[0]}', '{vat_number}', 1000, 'wheat', 'drip'),
        ('{ids[1]}', '{vat_number}', 2000, 'corn', 'drip'),
        ('{ids[2]}', '{vat_number}', 3000, 'rice', 'drip'),
        ('{ids[3]}', '{vat_number}', 4000, 'barley', 'drip'),
        ('{ids[4]}', '{vat_number}', 5000, 'grain', 'drip');            
    '''.format(ids=ids, vat_number=vat_number))

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
        "Should provide the expected list size",
        5,
        len(response.json())
    )

def test3(scope):
    scope.set_header('Should reject access to GET /field for WA role')

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
        "Should reject the request with a 403",
        403,
        response.status_code
    )
    Assertion.Equals(
        scope,
        "Should provide the expected message",
        "User unauthorized",
        response.text
    )

def test4(scope):
    scope.set_header('Should reject access to GET /field with invalid token(s)')

    keys = JWTRegistry.plainMappedKeys()
    sign_key = keys[0]["key"]

    cDae = CustomDate.parse(backendConfig["initial_date"])

    utc_date = cDae.epoch()

    jwt_payload = {}

    #sign jwt with custom iat time
    jwt_payload["iat"] = utc_date - 3600
    jwt_payload["exp"] = utc_date + 3600

    jwt_payload = {
        "company_vat_number": "test",
        "aud": backendConfig["aud"],
        "iss": backendConfig["iss"],
        #"sub": "test-user"
    }

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
        "Should reject if <sub> is missing",
        401,
        response.status_code
    )
    Assertion.Equals(
        scope,
        "Should provide the expected message",
        "Missing field(s) in token",
        response.text
    )


def test5(scope):
    scope.set_header('Test POST /field should succeed and create a new FarmField')

    fake = Faker('it_IT')
    Faker.seed(0)

    vat_number = fake.random_number(digits=11)

    conn = getPostgresConnection()
    cur = conn.cursor()
    cur.execute('''
        insert into company(vat_number, industry_sector) values('{vat_number}', 'FAR');
    '''.format(vat_number=vat_number))
    cur.execute('''
        insert into company_far(vat_number, industry_sector) values('{vat_number}', 'FAR');
    '''.format(vat_number=vat_number))

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


    response = requests.post(
        f"http://{backendConfig['host']}:{backendConfig['port']}/field",
        timeout=2,
        headers={
            "Authorization": f"Bearer {jwt}"
        },
        json={
            "square_meters": 1000,
            "crop_type": "wheat",
            "irrigation_type": "drip"
        }
    )
    Assertion.Equals(
        scope,
        "Should accept the request with a 200",
        200,
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
        1,
        len(result)
    )
    Assertion.Equals(
        scope,
        "Should provide the expected square meters",
        1000,
        result[0][1]
    )
    Assertion.Equals(
        scope,
        "Should provide the expected crop type",
        "wheat",
        result[0][2]
    )
    Assertion.Equals(
        scope,
        "Should provide the expected irrigation type",
        "drip",
        result[0][3]
    )

def test6(scope):
    scope.set_header('Test POST /field should succeed even when company has to be added')

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


    response = requests.post(
        f"http://{backendConfig['host']}:{backendConfig['port']}/field",
        timeout=2,
        headers={
            "Authorization": f"Bearer {jwt}"
        },
        json={
            "square_meters": 1000,
            "crop_type": "wheat",
            "irrigation_type": "drip"
        }
    )
    Assertion.Equals(
        scope,
        "Should accept the request with a 200",
        200,
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
        1,
        len(result)
    )
    Assertion.Equals(
        scope,
        "Should provide the expected square meters",
        1000,
        result[0][1]
    )
    Assertion.Equals(
        scope,
        "Should provide the expected crop type",
        "wheat",
        result[0][2]
    )
    Assertion.Equals(
        scope,
        "Should provide the expected irrigation type",
        "drip",
        result[0][3]
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
    suite.add_assertion(test4)
    suite.add_assertion(test5)
    suite.add_assertion(test6)
    suite.run()
    suite.print_stats()