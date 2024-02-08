from utility import TestSuite, Assertion
import psycopg2
import os
import requests
from utility import JWTRegistry

__location__ = os.path.realpath(
    os.path.join(os.getcwd(), os.path.dirname(__file__)))

databaseConfig = {
    "host": None,
    "port": None,
    "database": None,
    "user": None,
    "password": None
}

backendConfig = {
    "host": None,
    "port": None
}

def getPostgresConnection():
    return psycopg2.connect(
        host=databaseConfig["host"],
        database=databaseConfig["database"],
        user=databaseConfig["user"],
        password=databaseConfig["password"]
    )

def test1(scope):
    scope.set_header('Test user credentials to service application')
    response = requests.post(
        f"http://{backendConfig['host']}:{backendConfig['port']}/service/apply",
        json={
            "given_name": "Mickey",
            "family_name": "Doe",
            "email": "mickey.doe@gmail.com",
            "tax_code": "MCDMCK01A01F205Z",
            "company_vat_number": "12345678901",
            "company_category": "WA",
        },
        timeout=2
    )
    Assertion.Equals(
        scope,
        "Should reject if user is unlogged",
        401,
        response.status_code
    )
    Assertion.Equals(
        scope,
        "Should provide Missing authorization header message",
        "Missing authorization header",
        response.text
    )

def test2(scope):
    scope.set_header('Test user application to service')
    
    token = JWTRegistry.generate({
        "kid": "key1",
        "alg": "RS256",
        "typ": "JWT",
    }, {
        "sub": "1234567890",
        "given_name": "Mickey",
        "family_name": "Doe",
        "email": "mickey.doe@gmail.com",
        "iss": "https://appweb.andreabarchietto.it",
        "aud": "internal_workspace@appweb.andreabarchietto.it",
        "iat": 1316239022,
        "exp": 1899999999
    })
    response = requests.post(
        f"http://{backendConfig['host']}:{backendConfig['port']}/service/apply",
        json={
            "given_name": "Mickey",
            "family_name": "Doe",
            "email": "mickey.doe@gmail.com",
            "tax_code": "MCDMCK01A01F205Z",
            "company_vat_number": "12345678901",
            "company_category": "WA",
        },
        timeout=2,
        headers={
            "Authorization": f"bearer {token}"
        }
    )

    conn = getPostgresConnection()
    cur = conn.cursor()
    cur.execute('''
        delete from presentation_letter
    ''')
    cur.close()
    conn.commit()

    Assertion.Equals(
        scope,
        "Should accept the request",
        201,
        response.status_code
    )
    Assertion.Equals(
        scope,
        "Should provide the expected message",
        "Created",
        response.text
    )

def test3(scope):
    scope.set_header('Test user application twice')
    
    token = JWTRegistry.generate({
        "kid": "key1",
        "alg": "RS256",
        "typ": "JWT",
    }, {
        "sub": "1234567890",
        "given_name": "Mickey",
        "family_name": "Doe",
        "email": "mickey.doe@gmail.com",
        "iss": "https://appweb.andreabarchietto.it",
        "aud": "internal_workspace@appweb.andreabarchietto.it",
        "iat": 1316239022,
        "exp": 1899999999
    })
    response = requests.post(
        f"http://{backendConfig['host']}:{backendConfig['port']}/service/apply",
        json={
            "given_name": "Mickey",
            "family_name": "Doe",
            "email": "mickey.doe@gmail.com",
            "tax_code": "MCDMCK01A01F205Z",
            "company_vat_number": "12345678901",
            "company_category": "WA",
        },
        timeout=2,
        headers={
            "Authorization": f"bearer {token}"
        }
    )

    Assertion.Equals(
        scope,
        "Should accept the request the first time",
        201,
        response.status_code
    )
    Assertion.Equals(
        scope,
        "Should provide the expected message",
        "Created",
        response.text
    )

    response = requests.post(
        f"http://{backendConfig['host']}:{backendConfig['port']}/service/apply",
        json={
            "given_name": "Mickey",
            "family_name": "Doe",
            "email": "mickey.doe@gmail.com",
            "tax_code": "MCDMCK01A01F205Z",
            "company_vat_number": "12345678901",
            "company_category": "WA",
        },
        timeout=2,
        headers={
            "Authorization": f"bearer {token}"
        }
    )

    Assertion.Equals(
        scope,
        "Should reject the request the second time",
        400,
        response.status_code
    )
    Assertion.Equals(
        scope,
        "Should provide the expected message",
        "Application already exists",
        response.text
    )

    conn = getPostgresConnection()
    cur = conn.cursor()
    cur.execute('''
        delete from presentation_letter
    ''')
    cur.close()
    conn.commit()
    

def EntryPoint(
    database_ip,
    database_port,
    database_name,
    database_user,
    database_password,
    server_ip,
    server_port
):
    databaseConfig["host"] = database_ip
    databaseConfig["port"] = database_port
    databaseConfig["database"] = database_name
    databaseConfig["user"] = database_user
    databaseConfig["password"] = database_password
    backendConfig["host"] = server_ip
    backendConfig["port"] = server_port


    suite = TestSuite()
    suite.add_assertion(test1)
    suite.add_assertion(test2)
    suite.add_assertion(test3)
    suite.run()
    suite.print_stats()