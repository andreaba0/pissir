from .address_manager import address_manager

internalIp = address_manager.get_address()
internalPort = 8000

oauth_server_config = {
    "image_name": "appweb_fake_oauth_server",
    "environment": {
        "OAUTH_PROVIDER_PORT": internalPort,
        "OAUTH_BIND_IP": "0.0.0.0",
        "OAUTH_PROVIDER_DOMAIN": f"{internalIp}:{internalPort}",
    },
    "network": {
        "ip": internalIp,
        "name": address_manager.network_name
    },
    "internal_port": internalPort,
    "exposed_port": address_manager.get_port(),
}
        