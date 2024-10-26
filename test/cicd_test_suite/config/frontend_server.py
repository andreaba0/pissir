from .address_manager import address_manager
from .proxy_server import proxy_server_config
from utility.env import env_manager

from .proxy_server import proxy_server_config

internalIp = address_manager.get_address()
internalPort = 8000

frontend_server_config = {
    "image_name": "appweb_frontend_server",
    "environment": {
        "ipbackend_auth": f"http://{proxy_server_config['network']['ip']}:{proxy_server_config['internal_port']}/api/authorization",
        "ipbackend_api": f"http://{proxy_server_config['network']['ip']}:{proxy_server_config['internal_port']}/api/hydroservice",
        "googleClientId": env_manager.get("GOOGLE_CLIENT_ID"),
        "googleSecretId": env_manager.get("GOOGLE_CLIENT_SECRET"),
        "facebookClientId": env_manager.get("FACEBOOK_CLIENT_ID"),
        "facebookSecretId": env_manager.get("FACEBOOK_CLIENT_SECRET"),
        "listener_uri": "http://0.0.0.0:8000",
        "client_uri": f"http://localhost:{proxy_server_config['exposed_port']}/auth/SignIn",
        "oauth_uri": "https://appweb.andreabarchietto.it/localhost_redirect/oauth",
        "oauth_redirect_uri": "https://appweb.andreabarchietto.it/localhost_redirect/back",
        "oauthKey": "250bac54c19825467ac60a9dc7d70a54"
    },
    "network": {
        "ip": internalIp,
        "name": address_manager.network_name
    },
    "internal_port": internalPort,
    "exposed_port": address_manager.get_port(),
}
        