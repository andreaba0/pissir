from .address_manager import address_manager

api_database_config = {
    "image_name": "pissir_api_database",
    "environment": {
        "database_ip": None,
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