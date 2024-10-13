from .address_manager import address_manager
from utility.env import env_manager

from .proxy_server import proxy_server_config

internalIp = address_manager.get_address()
internalPort = 8000

frontend_server_config = {
    "image_name": "appweb_frontend_server",
    "environment": {
        "ipbackend": f"http://{proxy_server_config['network']['ip']}:{proxy_server_config['internal_port']}",
        "googleClientId": env_manager.get("GOOGLE_CLIENT_ID"),
        "googleSecretId": env_manager.get("GOOGLE_CLIENT_SECRET"),
        "facebookClientId": env_manager.get("FACEBOOK_CLIENT_ID"),
        "facebookSecretId": env_manager.get("FACEBOOK_CLIENT_SECRET"),
        "listener_uri": "http://0.0.0.0:8000"
    },
    "network": {
        "ip": internalIp,
        "name": address_manager.network_name
    },
    "internal_port": internalPort,
    "exposed_port": address_manager.get_port(),
}
        