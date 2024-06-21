from .address_manager import address_manager

oauth_server_config = {
    "image_name": "pissir_oauth_server",
    "environment": {
    },
    "network": {
        "ip": address_manager.get_address(),
        "name": address_manager.network_name
    },
    "internal_port": 8000,
    "exposed_port": address_manager.get_port(),
}
        