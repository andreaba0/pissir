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
    Assert.Contains(
        "Should provide the name of the missing field",
        ["company_category is required", "company_vat_number is required"],
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
    token ="eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCIsImtpZCI6InJzYWtleTEucGVtIn0.eyJpc3MiOiJodHRwczovL2FwcHdlYi5hbmRyZWFiYXJjaGlldHRvLml0IiwiYXVkIjoiaW50ZXJuYWxfd29ya3NwYWNlQGFwcHdlYi5hbmRyZWFiYXJjaGlldHRvLml0Iiwic3ViIjoiMjIyMzMzNDQ0IiwiZ2l2ZW5fbmFtZSI6Ik1pY2tleSIsImZhbWlseV9uYW1lIjoiRG9lIiwiZW1haWwiOiJtaWNrZXkuZG9lQGdtYWlsLmNvbSIsImlhdCI6MTUxNjIzOTAyMiwiZXhwIjoxODU1NTU1NTU1fQ.qrtQeNjnHXSW3UZ1c2wT_dUG5_vMCz2dP_zFpfjp5O3qTDqqzqlD966seBC_igjFIMOiNbuM9V95ChmpmrUrRjAN2g4WmaqAbwkla_sTxzIQRHSgBgVHzLemnbGiWLW5iIJh2zakBEjEB4Vx829qtLzxmo9fVMaQQoHb80eA6C3-GnBt9DiHpGhSsksDgcyhQHdLC9MCDDtmA7sMsdnP3vpfPgjcmZ0J-X1-6oyUtHrDrc3m7JXnCMULxRFD74wIoeCTMWMgU4dDvi7zyuLbxS9VibcS8dzcF13etThwzfeglwmJyYr2SEDN2lRsOsfnkZO12FyKHIrrpfEMAzvxBQ"
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

