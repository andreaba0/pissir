from config.frontend_server import frontend_server_config
import os
import time
from utility.env import env_manager
from utility.docker_lib import client

client = client.get_client()

class frontend_server:
    name = frontend_server_config["image_name"]

    def run(name_with_tag, env_list):
        print("Creating container")
        container_name = f"{frontend_server_config['image_name']}"
        container = client.containers.run(
            name_with_tag,
            name=container_name,
            environment=env_list,
            ports={f"{frontend_server_config['internal_port']}/tcp": frontend_server_config['exposed_port']},
            detach=True,
            labels={
                "com.pissir.env": "testing",
                "com.pissir.role": name_with_tag.split(":")[0],
            },
            remove=False
        )
        return container