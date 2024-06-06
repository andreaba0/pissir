from config.api_server import api_database_config

class api_database:
    name = api_database_config["image_name"]

    def build():
        return