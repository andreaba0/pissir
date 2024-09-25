from .address_manager import address_manager
from .api_database_server import api_database_config
from .mosquitto_server import mosquitto_server_config
from .oauth_server import oauth_server_config

internalPort = 8000

api_server_config = {
    "image_name": "pissir_api_server",
    "environment": {
        "DOTNET_ENV_WEBSERVER_BOUND": f"http://0.0.0.0:{internalPort}",
        "DOTNET_ENV_DATABASE_HOST": api_database_config["network"]["ip"],
        "DOTNET_ENV_DATABASE_PORT": api_database_config["internal_port"],
        "DOTNET_ENV_DATABASE_NAME": api_database_config["environment"]["POSTGRES_DB"],
        "DOTNET_ENV_DATABASE_USER": api_database_config["environment"]["POSTGRES_USER"],
        "DOTNET_ENV_DATABASE_PASSWORD": api_database_config["environment"]["POSTGRES_PASSWORD"],
        "DOTNET_ENV_MQTT_HOST": mosquitto_server_config["network"]["ip"],
        "DOTNET_ENV_MQTT_PORT": mosquitto_server_config["internal_port"],
        "DOTNET_ENV_MQTT_USER": "test",
        "DOTNET_ENV_MQTT_PASSWORD": "test",
        "DOTNET_ENV_MQTT_POOLSIZE": "5",
        "DOTNET_ENV_MQTT_PERCLIENTCAPACITY": "20",
        "DOTNET_ENV_AUTH_URI": f"http://{oauth_server_config['network']['ip']}:{oauth_server_config['internal_port']}",
        "DOTNET_ENV_PISSIR_ISS": "https://appweb.andreabarchietto.it",
        "DOTNET_ENV_PISSIR_AUD": "https://pissir.andreabarchietto.it",
        #"INITIAL_DATE": "01/01/2024 00:00:00",
        "INITIAL_DATE": "01/01/1970 00:00:00",
        "DOTNET_ENV_MQTT_HOST": mosquitto_server_config["network"]["ip"],
        "DOTNET_ENV_MQTT_PORT": mosquitto_server_config["internal_port"],
        "DOTNET_ENV_MQTT_USER": "mosquitto",
        "DOTNET_ENV_MQTT_PASSWORD": "password",
        "DOTNET_ENV_MQTT_POOLSIZE": "5",
        "DOTNET_ENV_MQTT_PERCLIENTCAPACITY": "15",
        "DOTNET_ENV_MQTT_TOPIC": "backend/measure/#"
    },
    "internal_port": internalPort,
    "exposed_port": address_manager.get_port(),
    "network": {
        "ip": address_manager.get_address(),
        "name": address_manager.network_name
    }
}
        