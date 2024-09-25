from utility import TestSuite, Assertion
import psycopg2
import os
import requests
from utility import JWTRegistry, UlidGenerator
import jose.jwt
from component.backend.api.utility.postgres import PostgresSuite
import datetime
import time

__location__ = os.path.realpath(
    os.path.join(os.getcwd(), os.path.dirname(__file__)))

databaseConfig = {
    "host": None,
    "port": None,
    "user": None,
    "password": None,
    "database": None
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

    jwt_payload = {
        "company_vat_number": "test",
        "role": "WA",
        "global_id": "5454t45t54t4",
        "aud": backendConfig["aud"],
        "iss": backendConfig["iss"]
    }
    keys = JWTRegistry.plainMappedKeys()
    sign_key = keys[0]["key"]

    #sign jwt with custom iat time
    jwt_payload["iat"] = int(time.time())-3600
    jwt_payload["exp"] = jwt_payload["iat"] + 3600 + 3600

    jwt = jose.jwt.encode(jwt_payload, sign_key, algorithm="RS256", headers={"kid": keys[0]["kid"]})
    print(jwt)
    
    scope.set_header('Test should get a list of all fields')
    response = requests.get(
        f"http://{backendConfig['host']}:{backendConfig['port']}/field",
        timeout=2,
        headers={
            "Authorization": f"Bearer {jwt}"
        }
    )
    Assertion.Equals(
        scope,
        "Should accept the request",
        200,
        response.status_code
    )
    Assertion.Equals(
        scope,
        "Should provide the expected message",
        "pong",
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


    suite = TestSuite()
    suite.set_tierup(TierUp)
    suite.set_tierdown(TierDown)
    suite.set_middletier(ClearDatabase)
    suite.add_assertion(test1)
    suite.run()
    suite.print_stats()