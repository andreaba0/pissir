from .address_manager import address_manager
from .api_database_server import api_database_config

api_server_config = {
    "image_name": "pissir_api_server",
    "environment": {
        "server_ip": None,
        "server_port": 8080,
        "database_ip": api_database_config["network"]["ip"],
        "database_port": 5432,
        "database_name": "api_db",
        "database_user": "postgres",
        "database_password": "password"  
    },
    "network": {
        "ip": address_manager.get_address(),
        "name": address_manager.network_name
    }
}
        