from .address_manager import address_manager
from .auth_database_server import auth_database_config

auth_server_config = {
    "image_name": "pissir_auth_server",
    "environment": {
        "DOTNET_ENV_DATABASE_HOST": auth_database_config["network"]["ip"],
        "DOTNET_ENV_DATABASE_PORT": auth_database_config["internal_port"],
        "DOTNET_ENV_DATABASE_USER": auth_database_config["environment"]["POSTGRES_USER"],
        "DOTNET_ENV_DATABASE_PASSWORD": auth_database_config["environment"]["POSTGRES_PASSWORD"],
        "DOTNET_ENV_DATABASE_NAME": auth_database_config["environment"]["POSTGRES_DB"],
        "DOTNET_ENV_WEBSERVER_BOUND": "http://0.0.0.0:8000",
        "DOTNET_ENV_PISSIR_ISS": "https://appweb.andreabarchietto.it",
        "DOTNET_ENV_PISSIR_AUD": "https://pissir.andreabarchietto.it",
        "INITIAL_DATE": "01/01/2024 00:00:00", 
    },
    "internal_port": 8000,
    "exposed_port": address_manager.get_port(),
    "network": {
        "ip": address_manager.get_address(),
        "name": address_manager.network_name
    }
}
        