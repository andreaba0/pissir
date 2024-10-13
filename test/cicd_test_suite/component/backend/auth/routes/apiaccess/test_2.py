from utility import TestSuite, Assertion, CustomDate
import psycopg2
import os
import requests
from utility import JWTRegistry
from faker import Faker
import datetime
import random
import uuid
from component.backend.auth.utility.postgres import PostgresSuite
from utility import JWTRegistry
import docker
from utility import AuthBackendContainer
from uuid import uuid4
from config.auth_server import auth_server_config

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
    "port": None,
    "initial_datetime": auth_server_config["environment"]["DOTNET_ENV_INITIAL_DATETIME"],
    "issuer": auth_server_config["environment"]["DOTNET_ENV_PISSIR_ISS"],
    "audience": auth_server_config["environment"]["DOTNET_ENV_PISSIR_AUD"]
}

class FakeCompany:
    def __init__(self, company):
        self.vat_number = company["vat_number"]
        self.industry_sector = company["industry_sector"]
        self.name = company["name"]
    def createCompany(faker, industry_sector):
        vat_number = faker.random_number(digits=11)
        return FakeCompany({
            "vat_number": str(vat_number),
            "industry_sector": industry_sector,
            "name": faker.company()
        })
    def getRole(self):
        if self.industry_sector == "FAR":
            return "FA"
        else:
            return "WA"

class FakeUser:
    def __init__(self, user):
        self.tax_code = user["ssn"]
        self.given_name = user["first_name"]
        self.family_name = user["last_name"]
        self.email = user["mail"]
        self.company= user["company"]
        self.sub = user["sub"]
        self.acl_requests = []
    def createUser(faker, company):
        first_name = faker.first_name()
        last_name = faker.last_name()
        fnamelower = first_name.lower()
        lnamelower = last_name.lower()
        sub = f"{fnamelower}.{lnamelower}@domain_test_service.io"
        mail = f"{fnamelower}.{lnamelower}@{faker.domain_name()}"
        return FakeUser({
            "ssn": faker.ssn(),
            "first_name": first_name,
            "last_name": last_name,
            "mail": mail,
            "company": company,
            "sub": sub
        })
    def internalId(self, internal_id):
        self.internal_id = internal_id
    def setGlobalId(self, global_id):
        self.global_id = global_id
    def addAclRequest(self, sdate, edate):
        self.acl_requests.append({
            "start_date": sdate,
            "end_date": edate
        })


def getPostgresConnection():
    return psycopg2.connect(
        host=databaseConfig["host"],
        port=databaseConfig["port"],
        database=databaseConfig["database"],
        user=databaseConfig["user"],
        password=databaseConfig["password"]
    )

class DateGenerator:
    initial = datetime.datetime(2022, 1, 1)
    def get():
        DateGenerator.initial += datetime.timedelta(days=1)
        return DateGenerator.initial

def test1(scope):
    scope.set_header('Test should get an access token')
    dateRequests = []

    #create 10 users profile with faker
    fake = Faker('it_IT')
    Faker.seed(0)

    users = []
    people = []
    company_far = FakeCompany.createCompany(fake, "FAR")
    company_wsp = FakeCompany.createCompany(fake, "WSP")
    user = FakeUser.createUser(fake, company_far)
    user_wsp = FakeUser.createUser(fake, company_wsp)

    conn = getPostgresConnection()
    cur = conn.cursor()
    cur.execute('''
        insert into company (vat_number, industry_sector) values
        ('{vat_number}', '{industry_sector}'),
        ('{vat_number2}', '{industry_sector2}')
        ON CONFLICT DO NOTHING
    '''.format(
        vat_number=user.company.vat_number, 
        industry_sector=user.company.industry_sector,
        vat_number2=company_wsp.vat_number,
        industry_sector2=company_wsp.industry_sector
    ))
    cur.execute('''
        insert into company_far (vat_number, industry_sector) values
        ('{vat_number}', 'FAR')
        ON CONFLICT DO NOTHING
    '''.format(vat_number=user.company.vat_number))
    cur.execute('''
        insert into company_wsp (vat_number, industry_sector) values
        ('{vat_number}', 'WSP')
        ON CONFLICT DO NOTHING
    '''.format(vat_number=company_wsp.vat_number))
    cur.execute('''
        insert into user_account (registered_provider, sub) values
        ('test_provider', '{user_sub}')
        returning id
    '''.format(user_sub=user.sub))
    user_id = cur.fetchone()[0]
    user.internalId(user_id)
    cur.execute('''
        insert into user_account (registered_provider, sub) values
        ('test_provider', '{user_sub}')
        returning id
    '''.format(user_sub=user_wsp.sub))
    user_wsp_id = cur.fetchone()[0]
    user_wsp.internalId(user_wsp_id)
    cur.execute('''
        insert into person (tax_code, account_id, given_name, family_name, email, person_role) values
        ('{user_taxcode}', {user_id}, '{user_fname}', '{user_lname}', '{user_mail}', '{user_role}'),
        ('{userwsp_taxcode}', {userwsp_id}, '{userwsp_fname}', '{userwsp_lname}', '{userwsp_mail}', '{userwsp_role}')
        returning global_id
    '''.format(
        user_taxcode=user.tax_code,
        user_id=user.internal_id,
        user_fname=user.given_name,
        user_lname=user.family_name,
        user_mail=user.email,
        user_role=user.company.getRole(),

        userwsp_taxcode=user_wsp.tax_code,
        userwsp_id=user_wsp.internal_id,
        userwsp_fname=user_wsp.given_name,
        userwsp_lname=user_wsp.family_name,
        userwsp_mail=user_wsp.email,
        userwsp_role=user_wsp.company.getRole()
    ))
    user.setGlobalId(cur.fetchone()[0])
    user_wsp.setGlobalId(cur.fetchone()[0])
    cur.execute('''
        insert into person_fa (account_id, role_name, company_vat_number) values
        ({user_id}, 'FA', '{vat_number}')
    '''.format(user_id=user.internal_id, vat_number=user.company.vat_number))
    cur.execute('''
        insert into person_wa (account_id, role_name, company_vat_number) values
        ({user_id}, 'WA', '{vat_number}')
    '''.format(user_id=user_wsp.internal_id, vat_number=company_wsp.vat_number))
    cur.execute('''
        insert into api_acl (person_fa, sdate, edate) values
        ({user_id}, to_date('2023-12-31', 'YYYY-MM-DD'), to_date('2024-01-02', 'YYYY-MM-DD'))
    '''.format(user_id=user.internal_id))


    cur.close()
    conn.commit()
    conn.close()

    date = CustomDate.parse(backendConfig["initial_datetime"])
    

    token = JWTRegistry.generate({
        "kid": "key1",
        "alg": "RS256",
        "typ": "JWT",
    }, {
        "sub": user.sub,
        "given_name": user.given_name,
        "family_name": user.family_name,
        "email": user.email,
        "iss": "https://appweb.andreabarchietto.it",
        "aud": "internal_workspace@appweb.andreabarchietto.it",
        "iat": date.epoch() - 3600,
        "exp": date.epoch() + 3600
    })
    response = requests.post(
        f"http://{backendConfig['host']}:{backendConfig['port']}/token",
        timeout=2,
        headers={
            "Authorization": f"bearer {token}"
        }
    )

    Assertion.Equals(
        scope,
        "Should return a success status code for FAR user",
        200,
        response.status_code
    )
    print(response.text)
    json = JWTRegistry.returnJsonPayload(response.text)
    Assertion.Equals(
        scope,
        "Should return the correct global id",
        user.global_id,
        json["sub"]
    )
    Assertion.Equals(
        scope,
        "Should return the correct company vat number",
        user.company.vat_number,
        json["company_vat_number"]
    )
    Assertion.Equals(
        scope,
        "Should return the correct role",
        user.company.getRole(),
        json["role"]
    )
    Assertion.Equals(
        scope,
        "Should return the correct issuer",
        backendConfig["issuer"],
        json["iss"]
    )
    Assertion.Equals(
        scope,
        "Should return the correct audience",
        backendConfig["audience"],
        json["aud"]
    )


    date = CustomDate.parse(backendConfig["initial_datetime"])

    token = JWTRegistry.generate({
        "kid": "key1",
        "alg": "RS256",
        "typ": "JWT",
    }, {
        "sub": user_wsp.sub,
        "given_name": user_wsp.given_name,
        "family_name": user_wsp.family_name,
        "email": user_wsp.email,
        "iss": "https://appweb.andreabarchietto.it",
        "aud": "internal_workspace@appweb.andreabarchietto.it",
        "iat": date.epoch() - 3600,
        "exp": date.epoch() + 3600
    })
    response = requests.post(
        f"http://{backendConfig['host']}:{backendConfig['port']}/token",
        timeout=2,
        headers={
            "Authorization": f"bearer {token}"
        }
    )

    Assertion.Equals(
        scope,
        "Should return a success status code for WSP user",
        200,
        response.status_code
    )
    json = JWTRegistry.returnJsonPayload(response.text)
    Assertion.Equals(
        scope,
        "Should return the correct global id",
        user_wsp.global_id,
        json["sub"]
    )
    Assertion.Equals(
        scope,
        "Should return the correct company vat number",
        user_wsp.company.vat_number,
        json["company_vat_number"]
    )
    Assertion.Equals(
        scope,
        "Should return the correct role",
        user_wsp.company.getRole(),
        json["role"]
    )
    Assertion.Equals(
        scope,
        "Should return the correct issuer",
        backendConfig["issuer"],
        json["iss"]
    )
    Assertion.Equals(
        scope,
        "Should return the correct audience",
        backendConfig["audience"],
        json["aud"]
    )




def uploadKeys():
    keys = JWTRegistry.uuidMappedKeys()
    conn = getPostgresConnection()
    cur = conn.cursor()
    for key_dict in keys:
        kid = key_dict["kid"]
        keyContent = key_dict["key"]
        cur.execute('''
            insert into rsa (
                id, key_content
            ) values (
                '{kid}',
                '{key_content}'
            )
            ON CONFLICT (id) DO NOTHING
        '''.format(
            kid=kid,
            key_content=keyContent
        ))
    cur.close()
    conn.commit()
    conn.close()

def setupContainer():
    uploadKeys()
    return
    client = docker.from_env()
    containerToRestart = "test_auth_server"
    container = client.containers.get(containerToRestart)
    container.restart()
    ct = AuthBackendContainer(container)
    ct.WaitTillRunning()
    ct.WaitRunningProcessOnPort("tcp", f"0.0.0.0:{backendConfig['port']}", "LISTEN")
    ct.WaitTillKeysAreDownloaded()
    print("Container restarted")




def TierUp():
    PostgresSuite.clearDatabase(getPostgresConnection())

def TierDown():
    PostgresSuite.clearDatabase(getPostgresConnection())

def ClearDatabase():
    PostgresSuite.clearDatabase(getPostgresConnection())
    

def EntryPoint(
    database_ip,
    database_port,
    database_name,
    database_user,
    database_password,
    server_ip,
    server_port,
    container
):
    databaseConfig["host"] = database_ip
    databaseConfig["port"] = database_port
    databaseConfig["database"] = database_name
    databaseConfig["user"] = database_user
    databaseConfig["password"] = database_password
    backendConfig["host"] = server_ip
    backendConfig["port"] = server_port

    #setupContainer()
    uploadKeys()
    ct = AuthBackendContainer(container)
    ct.WaitTillKeysAreDownloaded()


    suite = TestSuite()
    suite.set_tierup(TierUp)
    suite.set_tierdown(TierDown)
    suite.set_middletier(ClearDatabase)
    suite.add_assertion(test1)
    suite.run()
    suite.print_stats()