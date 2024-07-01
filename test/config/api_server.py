from .address_manager import address_manager
from .api_database_server import api_database_config

internalPort = 8080

api_server_config = {
    "image_name": "pissir_api_server",
    "environment": {
        "DOTNET_ENV_WEBSERVER_BOUND": f"http://0.0.0.0:{internalPort}",
        "DOTNET_ENV_DATABASE_HOST": api_database_config["network"]["ip"],
        "DOTNET_ENV_DATABASE_PORT": api_database_config["internal_port"],
        "DOTNET_ENV_DATABASE_NAME": api_database_config["environment"]["POSTGRES_DB"],
        "DOTNET_ENV_DATABASE_USER": api_database_config["environment"]["POSTGRES_USER"],
        "DOTNET_ENV_DATABASE_PASSWORD": api_database_config["environment"]["POSTGRES_PASSWORD"],
    },
    "internal_port": internalPort,
    "exposed_port": address_manager.get_port(),
    "network": {
        "ip": address_manager.get_address(),
        "name": address_manager.network_name
    }
}
        