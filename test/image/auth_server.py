from config.auth_server import auth_server_config
import os
import time
from utility.env import env_manager
from utility.docker_lib import client

client = client.get_client()

class auth_server:
    name = auth_server_config["image_name"]

    def run(env_list):
        container = client.containers.run(
            f"{auth_server.name}:1718983931",
            name= auth_server.name,
            environment=env_list,
            ports={f"{auth_server_config['internal_port']}/tcp": auth_server_config['exposed_port']},
            detach=True
        )
        return container