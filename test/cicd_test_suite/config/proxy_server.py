from .address_manager import address_manager

proxy_server_config = {
    "image_name": "appweb_proxy_server",
    "environment": {
        "ENVOY_LOG_LEVEL": "off",
    },
    "network": {
        "ip": address_manager.get_address(),
        "name": address_manager.network_name
    },
    "internal_port": 10000,
    "exposed_port": address_manager.get_port()
}