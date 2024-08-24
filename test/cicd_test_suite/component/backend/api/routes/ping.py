from utility import TestSuite, Assertion
import psycopg2
import os
import requests
from utility import JWTRegistry

__location__ = os.path.realpath(
    os.path.join(os.getcwd(), os.path.dirname(__file__)))

backendConfig = {
    "host": None,
    "port": None
}

def test1(scope):
    scope.set_header('Test user credentials to service application')
    response = requests.get(
        f"http://{backendConfig['host']}:{backendConfig['port']}/ping",
        timeout=2
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
    

def EntryPoint(
    *args,
    **kwargs
):
    server_ip = args[5]
    server_port = args[6]
    backendConfig["host"] = server_ip
    backendConfig["port"] = server_port


    suite = TestSuite()
    suite.add_assertion(test1)
    suite.run()
    suite.print_stats()