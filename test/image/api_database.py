from config.api_server import api_database_config
import os
import time
from utility.env import env_manager
from utility.docker_lib import client

client = client.get_client()

class api_database:
    name = api_database_config["image_name"]

    