from .address_manager import address_manager

proxy_server_config = {
    "image_name": "appweb_proxy_server",
    "environment": {
    },
    "network": {
        "ip": address_manager.get_address(),
        "name": address_manager.network_name
    },
    "dockerfile_path": "envoy/",
    "dockerfile_name": "Dockerfile"
}