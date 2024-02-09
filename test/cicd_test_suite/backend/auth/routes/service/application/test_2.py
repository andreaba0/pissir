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
    scope.set_header('Test user get his application')

    conn = getPostgresConnection()
    cur = conn.cursor()
    cur.execute('''
        insert into user_account (registered_provider, sub) values('test_provider', '1234567890')
        returning id
    ''')
    user_id = cur.fetchone()[0]
    cur.execute('''
        insert into presentation_letter
        (user_account, given_name, family_name, email, tax_code, company_vat_number, company_industry_sector)
        values
        ({user_id}, 'Mickey', 'Doe', 'mickey.doe@gmail.com', 'MCDMCK01A01F205Z', '12345678901', 'WSP')
        returning presentation_id
    '''.format(user_id=user_id))
    presentation_id = cur.fetchone()[0]
    cur.close()
    conn.commit()
    conn.close()
    
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
    response = requests.get(
        f"http://{backendConfig['host']}:{backendConfig['port']}/service/my_application",
        timeout=2,
        headers={
            "Authorization": f"bearer {token}"
        }
    )
    data = response.json()

    Assertion.Equals(
        scope,
        "Should return a success status code",
        200,
        response.status_code
    )

    Assertion.Equals(
        scope,
        "Should return the correct presentation letter",
        presentation_id,
        data["id"]
    )

    conn = getPostgresConnection()
    cur = conn.cursor()
    cur.execute('''delete from presentation_letter''')
    cur.execute('''delete from user_account''')
    cur.close()
    conn.commit()
    conn.close()

def test2(scope):
    scope.set_header('Test user get non existent application')

    conn = getPostgresConnection()
    cur = conn.cursor()
    cur.execute('''
        insert into user_account (registered_provider, sub) values('test_provider', '1234567890')
        returning id
    ''')
    user_id = cur.fetchone()[0]
    cur.close()
    conn.commit()
    conn.close()
    
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
    response = requests.get(
        f"http://{backendConfig['host']}:{backendConfig['port']}/service/my_application",
        timeout=2,
        headers={
            "Authorization": f"bearer {token}"
        }
    )
    data = response.text

    Assertion.Equals(
        scope,
        "Should return a not found status code",
        404,
        response.status_code
    )

    Assertion.Equals(
        scope,
        "Should return an explanation message",
        "Not Found",
        data
    )

    conn = getPostgresConnection()
    cur = conn.cursor()
    cur.execute('''delete from presentation_letter''')
    cur.execute('''delete from user_account''')
    cur.close()
    conn.commit()
    conn.close()

def clearDatabase():
    conn = getPostgresConnection()
    cur = conn.cursor()
    cur.execute('''delete from presentation_letter''')
    cur.execute('''delete from person_fa''')
    cur.execute('''delete from person_wa''')
    cur.execute('''delete from person''')
    cur.execute('''delete from company_far''')
    cur.execute('''delete from company_wsp''')
    cur.execute('''delete from company''')
    cur.execute('''delete from user_account''')
    cur.close()
    conn.commit()
    conn.close()

def TierUp():
    clearDatabase()

def TierDown():
    clearDatabase()
    

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
    suite.set_tierup(TierUp)
    suite.set_tierdown(TierDown)
    suite.add_assertion(test1)
    suite.add_assertion(test2)
    suite.run()
    suite.print_stats()