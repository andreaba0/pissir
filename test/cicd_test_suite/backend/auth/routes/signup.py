from utility import TestSuite, Assertion
import psycopg2
import os
import requests

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

def test1(scope):
    scope.set_header('Test 1')
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
    suite.run()
    suite.print_stats()