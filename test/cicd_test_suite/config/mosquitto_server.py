from .address_manager import address_manager

mosquitto_server_config = {
    "image_name": "pissir_broker_server",
    "environment": {
    },
    "internal_port": 1883,
    "exposed_port": address_manager.get_port(),
    "network": {
        "ip": address_manager.get_address(),
        "name": address_manager.network_name
    }
}
        