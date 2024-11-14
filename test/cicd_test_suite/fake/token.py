import os
import sys
import jose
from utility import CustomDate, JWTRegistry


    



def create(role):

    sub = ""
    vat_number = ""
    if role == "FA":
        sub="100156446789.far_sub.user@pissir.test.com"
        vat_number = "10015644678"
    elif role == "WA":
        sub="156432189054.wsp_sub.user@pissir.test.com"
        vat_number = "56432189054"
    
    if sub == "":
        print(f"Role: <{role}> not recognized")
        return

    jwt_payload = {
        "company_vat_number": str(vat_number),
        "role": "FA",
        "aud": "internal_workspace@appweb.andreabarchietto.it",
        "iss": "https://appweb.andreabarchietto.it",
        "sub": sub
    }
    keys = JWTRegistry.plainMappedKeys()
    sign_key = keys[0]["key"]

    cDae = CustomDate.today()
    utc_date = cDae.epoch()

    #sign jwt with custom iat time
    jwt_payload["iat"] = utc_date - 3600
    jwt_payload["exp"] = utc_date + 3600

    jwt = jose.jwt.encode(jwt_payload, sign_key, algorithm="RS256", headers={"kid": keys[0]["kid"]})

    print(jwt)

    

def fake_token_main(*args, **kwargs):
    create(args[0])