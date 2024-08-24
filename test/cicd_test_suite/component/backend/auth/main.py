import copy
import json
import psycopg2
import requests
import time


from cicd_test_suite.component.backend.auth.routes.service.application import test_1 as test_a1 
from cicd_test_suite.component.backend.auth.routes.service.application import test_2 as test_a2 
from cicd_test_suite.component.backend.auth.routes.service.application import test_3 as test_a3 
from cicd_test_suite.component.backend.auth.routes.service.application import test_4 as test_a4

from cicd_test_suite.component.backend.auth.routes.profile import test_1 as test_p1

from cicd_test_suite.component.backend.auth.routes.company import test_1 as test_c1
from cicd_test_suite.component.backend.auth.routes.company import test_2 as test_c2

from cicd_test_suite.component.backend.auth.routes.apiaccess import test_1 as test_api1
from cicd_test_suite.component.backend.auth.routes.apiaccess import test_2 as test_api2

from cicd_test_suite.utility.state import StateManager, Block, State

from config.auth_server import auth_server_config
from config.auth_database_server import auth_database_config
from config.oauth_server import oauth_server_config
from config.api_server import api_server_config
from config.api_database_server import api_database_config
from config.mosquitto_server import mosquitto_server_config
from config.proxy_server import proxy_server_config


from image.auth_server import auth_server
from image.auth_database import auth_database
from image.oauth_server import oauth_server
from image.api_server import api_server
from image.api_database import api_database
from image.mosquitto_server import mosquitto_server
from image.proxy_server import proxy_server

from utility import Container, AuthBackendContainer
from config.address_manager import address_manager



def EntryPoint(*args, **kwargs):
    test_a1.EntryPoint(*args, **kwargs)
    test_a2.EntryPoint(*args, **kwargs)
    test_a3.EntryPoint(*args, **kwargs)
    test_a4.EntryPoint(*args, **kwargs)
    test_p1.EntryPoint(*args, **kwargs)
    test_c1.EntryPoint(*args, **kwargs)
    test_c2.EntryPoint(*args, **kwargs)
    test_api1.EntryPoint(*args, **kwargs)
    #test_api2.EntryPoint(*args, **kwargs)

def checkAuthDatabaseConnectivity(container, config):
    ct = Container(container)
    ct.WaitTillRunning()
    internalPort = config["internal_port"]
    ct.WaitRunningProcessOnPort("tcp", f"0.0.0.0:{internalPort}", "LISTEN")
    ct.WaitIpAssignment(config["network"]["ip"], address_manager.get_netmask())
    ct.WaitPostgresStartingUp(config["network"]["ip"], config["internal_port"])

def checkAuthServerConnectivity(container, config):
    ct = Container(container)
    ct.WaitTillRunning()
    internalPort = config["internal_port"]
    ct.WaitRunningProcessOnPort("tcp", f"0.0.0.0:{internalPort}", "LISTEN")
    ct.WaitIpAssignment(config["network"]["ip"] , address_manager.get_netmask())

def checkOAuthServerConnectivity(container, config):
    ct = Container(container)
    ct.WaitTillRunning()
    internalPort = config["internal_port"]
    ct.WaitRunningProcessOnPort("tcp", f"0.0.0.0:{internalPort}", "LISTEN")
    ct.WaitIpAssignment(config["network"]["ip"], address_manager.get_netmask())

def initAuthDatabase(OAuthServerConfig, AuthDatabaseConfig):
    provider_uri = f"http://{OAuthServerConfig['network']['ip']}:{OAuthServerConfig['internal_port']}/.well-known/openid-configuration"
    #connect to database
    conn = psycopg2.connect(
        host="localhost",
        port=AuthDatabaseConfig["exposed_port"],
        database=AuthDatabaseConfig["environment"]["POSTGRES_DB"],
        user=AuthDatabaseConfig["environment"]["POSTGRES_USER"],
        password=AuthDatabaseConfig["environment"]["POSTGRES_PASSWORD"]
    )
    cur = conn.cursor()
    cur.execute('''
        insert into user_role (role_name) values('WA'), ('FA')
    ''')
    cur.execute('''
        insert into industry_sector(sector_name) values('WSP'), ('FAR')
    ''')
    cur.execute('''
        insert into registered_provider(provider_name, configuration_uri) values('test_provider', '{provider_uri}')
    '''.format(provider_uri=provider_uri))
    cur.execute('''
        insert into 
            allowed_audience(registered_provider, audience) 
        values
            ('test_provider', 'internal_workspace@appweb.andreabarchietto.it')
    ''')
    cur.close()
    conn.commit()
    conn.close()

def AuthMain():
    authServerConfig = copy.deepcopy(auth_server_config)
    authDatabaseConfig = copy.deepcopy(auth_database_config)
    oauthServerConfig = copy.deepcopy(oauth_server_config)
    containers = StateManager.converge([
        Block(auth_server, authServerConfig, State.CLEAR),
        Block(auth_database, authDatabaseConfig, State.NEW),
        Block(oauth_server, oauthServerConfig, State.CLEAR),
        Block(api_server, api_server_config, State.CLEAR),
        Block(api_database, api_database_config, State.CLEAR),
        Block(mosquitto_server, mosquitto_server_config, State.CLEAR),
        Block(proxy_server, proxy_server_config, State.CLEAR)
    ])
    checkAuthDatabaseConnectivity(containers[1], authDatabaseConfig)
    initAuthDatabase(oauthServerConfig, authDatabaseConfig)

    containers = StateManager.converge([
        Block(auth_server, authServerConfig, State.NEW),
        Block(auth_database, authDatabaseConfig, State.RUNNING),
        Block(oauth_server, oauthServerConfig, State.NEW),
        Block(api_server, api_server_config, State.CLEAR),
        Block(api_database, api_database_config, State.CLEAR),
        Block(mosquitto_server, mosquitto_server_config, State.CLEAR),
        Block(proxy_server, proxy_server_config, State.CLEAR)
    ])
    checkOAuthServerConnectivity(containers[2], oauthServerConfig)
    checkAuthServerConnectivity(containers[0], authServerConfig)
    ct = Container(containers[0])
    ct.WaitForStringInLogs(f"Updated RSA parameters for: {authServerConfig['environment']['DOTNET_ENV_PISSIR_ISS']}", 30)

    EntryPoint(
        "localhost",
        authDatabaseConfig["exposed_port"],
        authDatabaseConfig["environment"]["POSTGRES_DB"],
        authDatabaseConfig["environment"]["POSTGRES_USER"],
        authDatabaseConfig["environment"]["POSTGRES_PASSWORD"],
        "localhost",
        authServerConfig["exposed_port"]
    )

    containers = StateManager.converge([
        Block(auth_server, authServerConfig, State.NEW),
        Block(auth_database, authDatabaseConfig, State.RUNNING),
        Block(oauth_server, oauthServerConfig, State.RUNNING),
        Block(api_server, api_server_config, State.CLEAR),
        Block(api_database, api_database_config, State.CLEAR),
        Block(mosquitto_server, mosquitto_server_config, State.CLEAR),
        Block(proxy_server, proxy_server_config, State.CLEAR)
    ])
    checkAuthServerConnectivity(containers[0], authServerConfig)

    test_api2.EntryPoint(
        "localhost",
        authDatabaseConfig["exposed_port"],
        authDatabaseConfig["environment"]["POSTGRES_DB"],
        authDatabaseConfig["environment"]["POSTGRES_USER"],
        authDatabaseConfig["environment"]["POSTGRES_PASSWORD"],
        "localhost",
        authServerConfig["exposed_port"],
        containers[0]
    )