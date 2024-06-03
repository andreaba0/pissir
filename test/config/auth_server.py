from .address_manager import address_manager
from .auth_database_server import auth_database_config

auth_server_config = {
    "image_name": "pissir_auth_server",
    "environment": {
        "DOTNET_ENV_DATABASE_HOST": auth_database_config["network"]["ip"],
        "DOTNET_ENV_DATABASE_PORT": "5432",
        "DOTNET_ENV_DATABASE_USER": "postgres",
        "DOTNET_ENV_DATABASE_PASSWORD": "password",
        "DOTNET_ENV_DATABASE_NAME": "auth_db",
        "INITIAL_DATE": "01/01/2024 00:00:00", 
    },
    "network": {
        "ip": address_manager.get_address(),
        "name": address_manager.network_name
    }
}
        