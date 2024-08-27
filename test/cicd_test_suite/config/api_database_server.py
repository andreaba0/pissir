from .address_manager import address_manager

api_database_config = {
    "image_name": "pissir_api_database",
    "environment": {
        "POSTGRES_PASSWORD": "password",
        "POSTGRES_USER": "postgres",
        "POSTGRES_DB": "api_db"  
    },
    "internal_port": 5432,
    "exposed_port": address_manager.get_port(),
    "network": {
        "ip": address_manager.get_address(),
        "name": address_manager.network_name
    }
}