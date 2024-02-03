from utility import Assert, Group
import psycopg2
import requests
import os

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

def TierUpMethod():
    #read sql file setup.sql from current folder as string
    with open(os.path.join(__location__, 'setup.sql'), "r") as file:
        setupSql = file.read()
    setupSql = setupSql.replace("{{<provider_uri>}}", f"http://{backendConfig['host']}:{backendConfig['port']}/.well-known/openid-configuration")
    setupSql = setupSql.replace("{{<audience>}}", "pissir-test-suite")
    #connect to database
    conn = psycopg2.connect(
        host=databaseConfig["host"],
        database=databaseConfig["database"],
        user=databaseConfig["user"],
        password=databaseConfig["password"]
    )
    cur = conn.cursor()
    cur.execute(setupSql)
    conn.commit()
    cur.close()
    conn.close()

def TierDownMethod():
    #read sql file teardown.sql from current folder as string
    with open(os.path.join(__location__, "../clear.sql"), "r") as file:
        teardownSql = file.read()
    #connect to database
    conn = psycopg2.connect(
        host=databaseConfig["host"],
        database=databaseConfig["database"],
        user=databaseConfig["user"],
        password=databaseConfig["password"]
    )
    cur = conn.cursor()
    cur.execute(teardownSql)
    conn.commit()
    cur.close()
    conn.close()


def testWithUnloggedUser():
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
    Assert.Equals(
        "Should reject if user is unlogged",
        401,
        response.status_code
    )
    Assert.Equals(
        "Should provide Missing authorization header message",
        "Missing authorization header",
        response.text
    )

def testPartialBody():
    response = requests.post(
        f"http://{backendConfig['host']}:{backendConfig['port']}/service/apply",
        json={
            "given_name": "Mickey",
            "family_name": "Doe",
            "email": "mickey.doe@gmail.com",
            "tax_code": "MCDMCK01A01F205Z",
        },
        timeout=2
    )
    Assert.Equals(
        "Should reject if user is unlogged",
        400,
        response.status_code
    )
    Assert.Equals(
        "Should provide the name of the missing field",
        "company_vat_number is required",
        response.text
    )

def testWithMissingBody():
    response = requests.post(
        f"http://{backendConfig['host']}:{backendConfig['port']}/service/apply",
        timeout=2
    )
    Assert.Equals(
        "Should reject if user is unlogged",
        400,
        response.status_code
    )
    Assert.Equals(
        "Should provide expected json body message",
        "Expected json body",
        response.text
    )

def testSignupAccepted():
    token = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCIsImtpZCI6InJzYWtleTEucGVtIn0.eyJpc3MiOiJodHRwczovL2FwcHdlYi5hbmRyZWFiYXJjaGlldHRvLml0IiwiYXVkIjoicGlzc2lyLXRlc3Qtc3VpdGUiLCJzdWIiOiIyMjIzMzM0NDQiLCJnaXZlbl9uYW1lIjoiTWlja2V5IiwiZmFtaWx5X25hbWUiOiJEb2UiLCJlbWFpbCI6Im1pY2tleS5kb2VAZ21haWwuY29tIiwiaWF0IjoxNTE2MjM5MDIyLCJleHAiOjE4NTU1NTU1NTV9.Uzaa2H1yXanlIrYxqtc5p5YtLCkCjXrCE7xaLHP7_RDpHCcrQwWP_66-JzHwgOOp6wfUYU8X1TUGtRy_XYOks1L7r7feuB68QtaWuNmHQTOmybAcx74mHOqz7rB_zzTMJCykQCMfFsBfssM9Nhki3CI8_nIOeuhg8PVD9WAkESRiKOwGbV9oU294BhClGKzMYt_RkubDbmYiKiDIHfWQulQtc_BHdgQdZU-ohlmtDmcpPdDpoPJ0dlXTVJXWPuL3VHkh6RAvEOizrWOlXQ-31gf6pMV839XOSYCydP_6mPQ7UCF9nTXXHmHvdOT1eObio7PotijrHC-VnD5KDiVx8g"
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
    Assert.Equals(
        "Should accept the request",
        201,
        response.status_code
    )
    Assert.Equals(
        "Should provide the expected message",
        "Created",
        response.text
    )


def SignUpEntryPoint(
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


    testSuite = Group("Signup Test Suite")
    testSuite.TierUp(TierUpMethod)
    testSuite.Add(testWithUnloggedUser)
    testSuite.Add(testWithMissingBody)
    testSuite.Add(testPartialBody)
    testSuite.Add(testSignupAccepted)
    testSuite.TierDown(TierDownMethod)
    testSuite.Run()
    return

