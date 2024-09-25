import copy
import json
import psycopg2
import requests
import time

from utility.state import StateManager, Block, State

from config.auth_server import auth_server_config
from config.auth_database_server import auth_database_config
from config.oauth_server import oauth_server_config
from config.api_server import api_server_config
from config.api_database_server import api_database_config
from config.mosquitto_server import mosquitto_server_config
from config.proxy_server import proxy_server_config
from config.frontend_server import frontend_server_config


from image.auth_server import auth_server
from image.auth_database import auth_database
from image.oauth_server import oauth_server
from image.api_server import api_server
from image.api_database import api_database
from image.mosquitto_server import mosquitto_server
from image.proxy_server import proxy_server
from image.frontend import frontend_server

from utility import Container, AuthBackendContainer, JWTRegistry
from config.address_manager import address_manager


def checkDatabaseConnectivity(container, config):
    ct = Container(container)
    ct.WaitTillRunning()
    internalPort = config["internal_port"]
    ct.WaitRunningProcessOnPort("tcp", f"0.0.0.0:{internalPort}", "LISTEN")
    ct.WaitIpAssignment(config["network"]["ip"], address_manager.get_netmask())
    ct.WaitPostgresStartingUp(config["network"]["ip"], config["internal_port"])

def checkServerConnectivity(container, config):
    ct = Container(container)
    ct.WaitTillRunning()
    internalPort = config["internal_port"]
    ct.WaitRunningProcessOnPort("tcp", f"0.0.0.0:{internalPort}", "LISTEN")
    ct.WaitIpAssignment(config["network"]["ip"] , address_manager.get_netmask())

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

def uploadKeys(AuthServerConfig, AuthDatabaseConfig):
    keys = JWTRegistry.uuidMappedKeys()
    conn = psycopg2.connect(
        host="localhost",
        port=AuthDatabaseConfig["exposed_port"],
        database=AuthDatabaseConfig["environment"]["POSTGRES_DB"],
        user=AuthDatabaseConfig["environment"]["POSTGRES_USER"],
        password=AuthDatabaseConfig["environment"]["POSTGRES_PASSWORD"]
    )
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

def IntegrationMain():
    authServerConfig = copy.deepcopy(auth_server_config)
    authDatabaseConfig = copy.deepcopy(auth_database_config)
    apiServerConfig = copy.deepcopy(api_server_config)
    apiDatabaseConfig = copy.deepcopy(api_database_config)
    mosquittoServerConfig = copy.deepcopy(mosquitto_server_config)
    oauthServerConfig = copy.deepcopy(oauth_server_config)
    frontendServerConfig = copy.deepcopy(frontend_server_config)
    proxyServerConfig = copy.deepcopy(proxy_server_config)

    apiServerConfig["environment"]["DOTNET_ENV_AUTH_URI"] = f"http://{authServerConfig['network']['ip']}:{authServerConfig['internal_port']}"

    containers = StateManager.converge([
        Block(auth_server, authServerConfig, State.CLEAR),
        Block(auth_database, authDatabaseConfig, State.NEW),
        Block(oauth_server, oauthServerConfig, State.NEW),
        Block(api_server, apiServerConfig, State.CLEAR),
        Block(api_database, apiDatabaseConfig, State.NEW),
        Block(mosquitto_server, mosquittoServerConfig, State.NEW),
        Block(proxy_server, proxyServerConfig, State.NEW),
        Block(frontend_server, frontendServerConfig, State.NEW)
    ])
    checkDatabaseConnectivity(containers[1], authDatabaseConfig)
    uploadKeys(authServerConfig, authDatabaseConfig)
    checkDatabaseConnectivity(containers[4], apiDatabaseConfig)
    checkServerConnectivity(containers[2], oauthServerConfig)
    initAuthDatabase(oauthServerConfig, authDatabaseConfig)

    containers = StateManager.converge([
        Block(auth_server, authServerConfig, State.NEW),
        Block(auth_database, authDatabaseConfig, State.RUNNING),
        Block(oauth_server, oauthServerConfig, State.RUNNING),
        Block(api_server, apiServerConfig, State.NEW),
        Block(api_database, apiDatabaseConfig, State.RUNNING),
        Block(mosquitto_server, mosquittoServerConfig, State.RUNNING),
        Block(proxy_server, proxy_server_config, State.RUNNING),
        Block(frontend_server, frontendServerConfig, State.RUNNING)
    ])
    checkServerConnectivity(containers[0], authServerConfig)
    checkServerConnectivity(containers[3], apiServerConfig)
    ct = Container(containers[0])
    ct.WaitForStringInLogs(f"Updated RSA parameters for: {authServerConfig['environment']['DOTNET_ENV_PISSIR_ISS']}", 30)