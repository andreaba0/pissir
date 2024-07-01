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
        port=databaseConfig["port"],
        database=databaseConfig["database"],
        user=databaseConfig["user"],
        password=databaseConfig["password"]
    )

def test1(scope):
    scope.set_header('Test should get company partial information')

    conn = getPostgresConnection()
    cur = conn.cursor()
    cur.execute('''
        insert into user_account (registered_provider, sub) values
        ('test_provider', '1234567890')
        returning id
    ''')
    #get all returned ids
    user_id = cur.fetchone()[0]
    cur.execute('''
        insert into person (tax_code, account_id, given_name, family_name, email, person_role) values
        ('SMNMRZ01A01F205Z', {user_id4}, 'Sam', 'Marz', 'sam.marz@gmail.com', 'WA')
        returning global_id
    '''.format(user_id4=user_id))
    global_id = cur.fetchone()[0]
    cur.execute('''
        insert into company (vat_number, industry_sector) values
        ('89012345678', 'WSP')
    ''')
    cur.execute('''
        insert into company_wsp (vat_number, industry_sector) values
        ('89012345678', 'WSP')
    ''')
    cur.execute('''
        insert into person_wa (account_id, role_name, company_vat_number) values
        ({user_id}, 'WA', '89012345678')
    '''.format(user_id=user_id))
    cur.close()
    conn.commit()
    conn.close()
    
    token = JWTRegistry.generate({
        "kid": "key1",
        "alg": "RS256",
        "typ": "JWT",
    }, {
        "sub": "1234567890",
        "given_name": "Sam",
        "family_name": "Marz",
        "email": "sam.marz@gmail.com",
        "iss": "https://appweb.andreabarchietto.it",
        "aud": "internal_workspace@appweb.andreabarchietto.it",
        "iat": 1316239022,
        "exp": 1899999999
    })
    response = requests.get(
        f"http://{backendConfig['host']}:{backendConfig['port']}/company",
        timeout=2,
        headers={
            "Authorization": f"bearer {token}"
        }
    )

    Assertion.Equals(
        scope,
        "Should return a success status code",
        200,
        response.status_code
    )

    data = response.json()

    Assertion.Equals(
        scope,
        "Should return company vat_number",
        "89012345678",
        data["vat_number"]
    )

    Assertion.Equals(
        scope,
        "Should return null for company name",
        None,
        data["company_name"]
    )

def test2(scope):
    scope.set_header('Test should return full company information')

    conn = getPostgresConnection()
    cur = conn.cursor()
    cur.execute('''
        insert into user_account (registered_provider, sub) values
        ('test_provider', '1234567890')
        returning id
    ''')
    #get all returned ids
    user_id = cur.fetchone()[0]
    cur.execute('''
        insert into person (tax_code, account_id, given_name, family_name, email, person_role) values
        ('SMNMRZ01A01F205Z', {user_id4}, 'Sam', 'Marz', 'sam.marz@gmail.com', 'WA')
        returning global_id
    '''.format(user_id4=user_id))
    global_id = cur.fetchone()[0]
    cur.execute('''
        insert into company (
            vat_number, 
            industry_sector,
            company_name,
            working_address,
            working_email_address,
            working_phone_number
        ) values
        ('89012345678', 'WSP', 'Water Provider Spa', 'Via Roma 10', 'info@waterproviderspa.com', '3331112229')
    ''')
    cur.execute('''
        insert into company_wsp (vat_number, industry_sector) values
        ('89012345678', 'WSP')
    ''')
    cur.execute('''
        insert into person_wa (account_id, role_name, company_vat_number) values
        ({user_id}, 'WA', '89012345678')
    '''.format(user_id=user_id))
    cur.close()
    conn.commit()
    conn.close()
    
    token = JWTRegistry.generate({
        "kid": "key1",
        "alg": "RS256",
        "typ": "JWT",
    }, {
        "sub": "1234567890",
        "given_name": "Sam",
        "family_name": "Marz",
        "email": "sam.marz@gmail.com",
        "iss": "https://appweb.andreabarchietto.it",
        "aud": "internal_workspace@appweb.andreabarchietto.it",
        "iat": 1316239022,
        "exp": 1899999999
    })
    response = requests.get(
        f"http://{backendConfig['host']}:{backendConfig['port']}/company",
        timeout=2,
        headers={
            "Authorization": f"bearer {token}"
        }
    )

    Assertion.Equals(
        scope,
        "Should return a success status code",
        200,
        response.status_code
    )

    data = response.json()

    Assertion.Equals(
        scope,
        "Should expect the correct company vat_number",
        "89012345678",
        data["vat_number"]
    )

    Assertion.Equals(
        scope,
        "Should expect the correct company name",
        "Water Provider Spa",
        data["company_name"]
    )

    Assertion.Equals(
        scope,
        "Should expect the correct company industry_sector",
        "WSP",
        data["industry_sector"]
    )

def clearDatabase():
    conn = getPostgresConnection()
    cur = conn.cursor()
    cur.execute('''delete from presentation_letter''')
    cur.execute('''delete from api_acl''')
    cur.execute('''delete from api_acl_request''')
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
    suite.set_middletier(clearDatabase)
    suite.add_assertion(test1)
    suite.add_assertion(test2)
    suite.run()
    suite.print_stats()