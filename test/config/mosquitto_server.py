from .address_manager import address_manager

mosquitto_server_config = {
    "image_name": "pissir_mosquitto_server",
    "environment": {
    },
    "network": {
        "ip": address_manager.get_address(),
        "name": address_manager.network_name
    }
}
        