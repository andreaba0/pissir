from config.mosquitto_server import mosquitto_server_config
import os
import time
from utility.env import env_manager
from utility.docker_lib import client

client = client.get_client()

class mosquitto_server:
    name = mosquitto_server_config["image_name"]

    def run(name_with_tag, env_list):
        print("Creating container")
        # container_name = f"{auth_server_config['image_name']}_{actual time in seconds}"
        container_name = f"{mosquitto_server_config['image_name']}_{int(time.time())}"
        container = client.containers.run(
            name_with_tag,
            #name=container_name,
            environment=env_list,
            ports={f"{mosquitto_server_config['internal_port']}/tcp": mosquitto_server_config['exposed_port']},
            detach=True,
            labels={
                "com.pissir.env": "testing",
                "com.pissir.role": name_with_tag.split(":")[0],
            },
            remove=False
        )
        return container