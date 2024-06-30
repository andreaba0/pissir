import psycopg2
import getpass
import json
import base64
import sys

postgresConfig = {
    "host": None,
    "port": None,
    "database": None,
    "user": None,
    "password": None
}

def getPostgresConnection():
    print("Connecting to postgres {host}:{port}".format(host=postgresConfig["host"], port=postgresConfig["port"]))
    conn = psycopg2.connect(
        host=postgresConfig["host"],
        port=postgresConfig["port"],
        database=postgresConfig["database"],
        user=postgresConfig["user"],
        password=postgresConfig["password"]
    )
    return conn

def getPayload(jwt):
    (header, payload, signature) = jwt.split(".")
    payload = payload + "=" * (4 - len(payload) % 4)
    payload = base64.b64decode(payload).decode("utf-8")
    payload = json.loads(payload)
    return payload


def authorize(jwt, provider):
    payload = getPayload(jwt)
    print(payload)
    conn = getPostgresConnection()
    cur = conn.cursor()
    cur.execute('''
        delete from presentation_letter
        where user_account = (
            select id from user_account
            where sub = '{sub}' and registered_provider='{registered_provider}'
        )
        returning user_account, given_name, family_name, email, tax_code, company_vat_number, company_industry_sector
    '''.format(sub=payload["sub"], registered_provider=provider))
    if cur.rowcount != 1:
        raise Exception('''Expected 1 row but got {rowcount} rows'''.format(rowcount=cur.rowcount))
    row = cur.fetchone()
    user_account = row[0]
    given_name = row[1]
    family_name = row[2]
    email = row[3]
    tax_code = row[4]
    company_vat_number = row[5]
    company_industry_sector = row[6]
    cur.execute('''
        insert into company(vat_number, industry_sector)
        values('{company_vat_number}', '{company_industry_sector}')
        on conflict(vat_number) do nothing
    '''.format(company_vat_number=company_vat_number, company_industry_sector=company_industry_sector))
    company_table = "company_far" if company_industry_sector == "FAR" else "company_wsp"
    cur.execute('''
        insert into {company_table}(vat_number, industry_sector)
        values('{company_vat_number}', '{company_industry_sector}')
    '''.format(company_vat_number=company_vat_number, company_industry_sector=company_industry_sector, company_table=company_table))
    person_role = "FA" if company_industry_sector == "FAR" else "WA"
    cur.execute('''
        insert into person(account_id, given_name, family_name, email, tax_code, person_role) values
        ('{account_id}', '{given_name}', '{family_name}', '{email}', '{tax_code}', '{person_role}')
    '''.format(account_id=user_account, given_name=given_name, family_name=family_name, email=email, tax_code=tax_code, person_role=person_role))
    person_table = "person_fa" if company_industry_sector == "FAR" else "person_wa"
    cur.execute('''
        insert into {person_table} (account_id, role_name, company_vat_number) values
        ({user_account}, '{person_role}', '{company_vat_number}')
    '''.format(person_table=person_table, user_account=user_account, person_role=person_role, company_vat_number=company_vat_number))
    cur.close()
    conn.commit()
    conn.close()
    print("User authorized")
    return



#This is a standalone script to automatically approve a user into the system.
#The only input required is the jwt and the provider registered by the user in the database.
def main(args=sys.argv):
    postgresConfig["password"] = getpass.getpass(prompt="Postgres password: ")
    # args is expecting -d, -h, -U, -p for database, host, user, and port respectively
    for i in range(len(args)):
        if args[i] == "-d":
            postgresConfig["database"] = args[i+1]
        if args[i] == "-h":
            postgresConfig["host"] = args[i+1]
        if args[i] == "-U":
            postgresConfig["user"] = args[i+1]
        if args[i] == "-p":
            postgresConfig["port"] = args[i+1]

    jwt = input("Enter jwt: ")
    provider = input("Enter provider: ")
    authorize(jwt, provider)
    return

if __name__ == "__main__":
    main()