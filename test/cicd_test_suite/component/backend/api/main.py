import copy
from pytz import timezone
import datetime
import pytz


from component.backend.api.routes import ping as test_ping
from component.backend.api.routes import field as test_field
from component.backend.api.routes import crops as test_crops
from component.backend.api.routes import irrigation as test_irrigation
from component.backend.api.routes.r_water import stock as test_water_stock
from component.backend.api.routes.r_field import route_field_id as test_field_route_field_id
from component.backend.api.routes.r_water import offer as test_water_offer
from component.backend.api.routes.r_water.r_limit import all as test_water_limit_all
from component.backend.api.routes.r_water import order as test_water_order
from component.backend.api.routes.r_water.r_recommendation import route_field_id as test_water_recommendation_route_field_id
from component.backend.api.routes.r_water import limit as test_water_limit
from component.backend.api.routes.r_company import secret as test_company_secret
from component.backend.api.routes.r_water import buy as test_water_buy
from component.backend.api.routes.r_water import consumption as test_water_consumption
from component.backend.api.routes.r_resourcemanager import field as test_resourcemanager_field
from component.backend.api.routes.r_resourcemanager.r_water.r_stock import route_field_id as test_resourcemanager_water_stock_route_field_id

from utility.state import StateManager, Block, State

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
    test_ping.EntryPoint(*args, **kwargs)
    test_field.EntryPoint(*args, **kwargs)
    test_crops.EntryPoint(*args, **kwargs)
    test_irrigation.EntryPoint(*args, **kwargs)
    test_water_stock.EntryPoint(*args, **kwargs)
    test_field_route_field_id.EntryPoint(*args, **kwargs)
    test_water_offer.EntryPoint(*args, **kwargs)
    test_water_limit_all.EntryPoint(*args, **kwargs)
    test_water_order.EntryPoint(*args, **kwargs)
    test_water_recommendation_route_field_id.EntryPoint(*args, **kwargs)
    test_water_limit.EntryPoint(*args, **kwargs)
    test_company_secret.EntryPoint(*args, **kwargs)
    test_water_buy.EntryPoint(*args, **kwargs)
    test_water_consumption.EntryPoint(*args, **kwargs)
    test_resourcemanager_field.EntryPoint(*args, **kwargs)
    test_resourcemanager_water_stock_route_field_id.EntryPoint(*args, **kwargs)

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
    ct.WaitForStringInLogs(f"Updated RSA parameters for: {config['environment']['DOTNET_ENV_PISSIR_ISS']}", 30)

def checkOAuthServerConnectivity(container, config):
    ct = Container(container)
    ct.WaitTillRunning()
    internalPort = config["internal_port"]
    ct.WaitRunningProcessOnPort("tcp", f"0.0.0.0:{internalPort}", "LISTEN")
    ct.WaitIpAssignment(config["network"]["ip"], address_manager.get_netmask())

def checkMosquittoServerConnectivity(container, config):
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


    # Just for testing purposes api database, broker server, and fake_oauth server are run first
    # This is to ensure that they are available to connect instantly when the api server is run
    # This facilitates the testing of the api server (so, only api can be tested without worrying about other services availability)

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
    checkMosquittoServerConnectivity(containers[5], mosquitto_server_config)
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
        apiServerConfig["exposed_port"],
        apiServerConfig["environment"]["DOTNET_ENV_PISSIR_ISS"],
        apiServerConfig["environment"]["DOTNET_ENV_PISSIR_AUD"],
        apiServerConfig["environment"]["DOTNET_ENV_INITIAL_DATETIME"]
    )