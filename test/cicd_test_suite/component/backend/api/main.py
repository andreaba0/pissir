import copy
import json
import psycopg2
import requests
import time


from cicd_test_suite.component.backend.api.routes import ping as test_a1

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

def checkApiDatabaseConnectivity(container, config):
    ct = Container(container)
    ct.WaitTillRunning()
    internalPort = config["internal_port"]
    ct.WaitRunningProcessOnPort("tcp", f"0.0.0.0:{internalPort}", "LISTEN")
    ct.WaitIpAssignment(config["network"]["ip"], address_manager.get_netmask())
    ct.WaitPostgresStartingUp(config["network"]["ip"], config["internal_port"])

def checkApiServerConnectivity(container, config):
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

def initApiDatabase(OAuthServerConfig, ApiDatabaseConfig):
    return

def ApiMain():
    apiServerConfig = copy.deepcopy(api_server_config)
    apiDatabaseConfig = copy.deepcopy(api_database_config)
    oauthServerConfig = copy.deepcopy(oauth_server_config)
    containers = StateManager.converge([
        Block(auth_server, auth_server_config, State.CLEAR),
        Block(auth_database, auth_database_config, State.CLEAR),
        Block(oauth_server, oauthServerConfig, State.NEW),
        Block(api_server, apiServerConfig, State.CLEAR),
        Block(api_database, apiDatabaseConfig, State.NEW),
        Block(mosquitto_server, mosquitto_server_config, State.NEW),
        Block(proxy_server, proxy_server_config, State.CLEAR)
    ])
    checkApiDatabaseConnectivity(containers[4], apiDatabaseConfig)
    initApiDatabase(oauthServerConfig, apiDatabaseConfig)
    checkOAuthServerConnectivity(containers[2], oauthServerConfig)

    containers = StateManager.converge([
        Block(auth_server, auth_server_config, State.CLEAR),
        Block(auth_database, auth_database_config, State.CLEAR),
        Block(oauth_server, oauthServerConfig, State.RUNNING),
        Block(api_server, apiServerConfig, State.NEW),
        Block(api_database, apiDatabaseConfig, State.RUNNING),
        Block(mosquitto_server, mosquitto_server_config, State.RUNNING),
        Block(proxy_server, proxy_server_config, State.CLEAR)
    ])
    checkApiServerConnectivity(containers[3], apiServerConfig)

    EntryPoint(
        "localhost",
        apiDatabaseConfig["exposed_port"],
        apiDatabaseConfig["environment"]["POSTGRES_DB"],
        apiDatabaseConfig["environment"]["POSTGRES_USER"],
        apiDatabaseConfig["environment"]["POSTGRES_PASSWORD"],
        "localhost",
        apiServerConfig["exposed_port"]
    )