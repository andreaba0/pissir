from utility import TestSuite, Assertion, CustomDate, Encode
import psycopg2
import os
import requests
from utility import JWTRegistry, UlidGenerator
import jose
from component.backend.api.utility.postgres import PostgresSuite
import datetime
import time
import json
from faker import Faker

__location__ = os.path.realpath(
    os.path.join(os.getcwd(), os.path.dirname(__file__)))

databaseAuthConfig = {
    "host": None,
    "port": None,
    "user": None,
    "password": None,
    "database": None
}

databaseApiConfig = {
    "host": None,
    "port": None,
    "user": None,
    "password": None,
    "database": None
}

def getPostgresAuthConnection():
    return psycopg2.connect(
        host=databaseAuthConfig["host"],
        port=databaseAuthConfig["port"],
        database=databaseAuthConfig["database"],
        user=databaseAuthConfig["user"],
        password=databaseAuthConfig["password"]
    )

def getPostgresApiConnection():
    return psycopg2.connect(
        host=databaseApiConfig["host"],
        port=databaseApiConfig["port"],
        database=databaseApiConfig["database"],
        user=databaseApiConfig["user"],
        password=databaseApiConfig["password"]
    )


def prepopulate():

    fake = Faker('it_IT')
    Faker.seed(0)

    vat_number_far = "10015644678"
    vat_number_wsp = "56432189054"
    offer_ids = [UlidGenerator.generate() for _ in range(2)]
    field_id = UlidGenerator.generate()
    company_secret = "yXab93xC92s4sGjH"
    date_minus1 = CustomDate.today().addDays(-1).toISODate()
    date_minus2 = CustomDate.today().addDays(-2).toISODate()
    date_minus3 = CustomDate.today().addDays(-3).toISODate()
    today = CustomDate.today().toISODate()

    conn = getPostgresApiConnection()
    cur = conn.cursor()
    cur.execute('''
        insert into company(vat_number, industry_sector) values('{vat_number}', 'FAR'), ('{vat_number_wsp}', 'WSP');
    '''.format(vat_number=vat_number_far, vat_number_wsp=vat_number_wsp))
    cur.execute('''
        insert into company_far(vat_number, industry_sector) values('{vat_number}', 'FAR');
    '''.format(vat_number=vat_number_far))
    cur.execute('''
        insert into company_wsp(vat_number, industry_sector) values('{vat_number}', 'WSP');
    '''.format(vat_number=vat_number_wsp))
    cur.execute('''
        insert into farm_field(id, vat_number) values
        ('{field_id}', '{vat_number}');
    '''.format(field_id=field_id, vat_number=vat_number_far))
    cur.execute('''
        insert into farm_field_versioning(field_id, vat_number, square_meters, crop_type, irrigation_type, created_at) values
        ('{id}', '{vat_number}', 1000, 'wheat', 'drip', '{date_minus3}'),
        ('{id}', '{vat_number}', 2000, 'corn', 'flood', '{date_minus2}'),
        ('{id}', '{vat_number}', 3000, 'rice', 'subsurface', '{date_minus1}'); 
    '''.format(id=field_id, vat_number=vat_number_far, date_minus1=date_minus1, date_minus2=date_minus2, date_minus3=date_minus3))
    cur.execute('''
        insert into offer(id, vat_number, publish_date, price_liter, available_liters, purchased_liters) values
        ('{ids[0]}', '{wa_vat_number}', '{today}', 0.5, 1000, 100),
        ('{ids[1]}', '{wa_vat_number}', '{today}', 0.6, 2000, 200);
    '''.format(ids=offer_ids, wa_vat_number=vat_number_wsp, today=today))
    cur.execute('''
        insert into buy_order(offer_id, farm_field_id, qty) values
        ('{offer_ids[0]}', '{field_id}', 100),
        ('{offer_ids[1]}', '{field_id}', 200);
    '''.format(offer_ids=offer_ids, field_id=field_id))
    cur.execute('''
        insert into secret_key(company_vat_number, secret_key) values('{vat_number}', '{company_secret}');
    '''.format(vat_number=vat_number_far, company_secret=company_secret))
    cur.close()
    conn.commit()
    conn.close()


    far_sub = "100156446789.far_sub.user@pissir.test.com"
    wsp_sub = "156432189054.wsp_sub.user@pissir.test.com"

    conn = getPostgresAuthConnection()
    cur = conn.cursor()
    cur.execute('''
        insert into user_account (registered_provider, sub) values
        ('test_provider', '{far_sub}'), ('test_provider', '{wsp_sub}')
        returning id
    '''.format(far_sub=far_sub, wsp_sub=wsp_sub))
    #get all returned ids
    user_ids = cur.fetchall()
    cur.execute('''
        insert into person (tax_code, account_id, given_name, family_name, email, person_role) values
        ('SMTMRK01A01D105Z', {user_id_fa}, 'Mark', 'Smith', 'mark.smith@gmail.com', 'FA'),
        ('SMNMRZ01A01F205Z', {user_id_wa}, 'Sam', 'Marz', 'sam.marz@gmail.com', 'WA')
        returning global_id
    '''.format(user_id_fa=user_ids[0][0], user_id_wa=user_ids[1][0]))
    global_ids = cur.fetchall()
    cur.execute('''
        insert into company (vat_number, industry_sector) values
        ('{vat_number_far}', 'FAR'), ('{vat_number_wsp}', 'WSP')
    ''' .format(vat_number_far=vat_number_far, vat_number_wsp=vat_number_wsp))
    cur.execute('''
        insert into company_wsp (vat_number, industry_sector) values
        ('{vat_number}', 'WSP')
    ''' .format(vat_number=vat_number_wsp))
    cur.execute('''
        insert into company_far (vat_number, industry_sector) values
        ('{vat_number}', 'FAR')
    ''' .format(vat_number=vat_number_far))
    cur.execute('''
        insert into person_wa (account_id, role_name, company_vat_number) values
        ({user_id}, 'WA', '{vat_number}')
    '''.format(user_id=user_ids[1][0], vat_number=vat_number_wsp))
    cur.execute('''
        insert into person_fa (account_id, role_name, company_vat_number) values
        ({user_id}, 'FA', '{vat_number}')
    '''.format(user_id=user_ids[0][0], vat_number=vat_number_far))
    cur.close()
    conn.commit()
    conn.close()

def EntryPoint(
    *args,
    **kwargs
):
    databaseAuthConfig["host"] = args[0]
    databaseAuthConfig["port"] = args[1]
    databaseAuthConfig["user"] = args[2]
    databaseAuthConfig["password"] = args[3]
    databaseAuthConfig["database"] = args[4]

    databaseApiConfig["host"] = args[5]
    databaseApiConfig["port"] = args[6]
    databaseApiConfig["user"] = args[7]
    databaseApiConfig["password"] = args[8]
    databaseApiConfig["database"] = args[9]

    prepopulate()

