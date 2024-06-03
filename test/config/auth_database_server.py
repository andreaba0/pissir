from .address_manager import address_manager

auth_database_config = {
    "image_name": "pissir_auth_database",
    "environment": {
        "POSTGRES_PASSWORD": "password",
        "POSTGRES_USER": "postgres",
        "POSTGRES_DB": "auth_db" 
    },
    "network": {
        "ip": address_manager.get_address(),
        "name": address_manager.network_name
    },
    "dockerfile_path": "database/auth/",
    "dockerfile_name": "Dockerfile"
}