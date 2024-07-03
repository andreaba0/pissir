from image import api_database
from image import api_server
from image import mosquitto_server
from image import auth_server
from image import auth_database
from image import oauth_server
from image import proxy_server

import json

from cicd_test_suite.utility import docker_lib

client = docker_lib.client.get_client()

from config.auth_server import auth_server_config
from config.mosquitto_server import mosquitto_server_config
from config.auth_database_server import auth_database_config
from config.oauth_server import oauth_server_config

class custom_env_routine:

    containers = {}

    def list_running_containers():
        custom_env_routine.containers = {}
        ls_raw = client.containers.list(
            all=False
        )
        # return object: [{id, name, image, status, bridge_ip}]
        # to get bridge_ip it is required to run inspect on container for each container
        ls = []
        for container in ls_raw:
            container_data = client.containers.get(container.id)
            if container_data.labels.get("com.pissir.role") == None and container_data.labels.get("com.pissir.env") == None:
                    continue
            container_name = container_data.labels["com.pissir.role"]
            if container_name in custom_env_routine.containers:
                custom_env_routine.containers[container_name].append(container_data)
            else:
                custom_env_routine.containers[container_name] = [container_data]
            continue
            bridge_ip = container_data.attrs["NetworkSettings"]["Networks"]["bridge"]["IPAddress"]
            image_name = container_data.labels["com.pissir.role"]
            if image_name in custom_env_routine.images:   
                custom_env_routine.images[image_name]["bridge_ip"].append(bridge_ip)
                custom_env_routine.images[image_name]["id"].append(container.id)
            else:
                custom_env_routine.images[image_name] = {
                    "id": [container.id],
                    "name": container.name,
                    "status": container.status,
                    "bridge_ip": [bridge_ip]
                }
    
    def container_template(index, container_name):
        cnt = custom_env_routine.containers.get(container_name)
        if cnt is None:
            return f"{index}. [ offline ] {container_name} Press {index} to run"
        ip_list = []
        status_list = []
        for container in cnt:
            ip_list.append(container.attrs["NetworkSettings"]["Networks"]["bridge"]["IPAddress"])
            status_list.append(container.status)
        ip_list = ", ".join(ip_list)
        status_list = ", ".join(status_list)
        return f"{index}. [ {status_list} ] {container_name} [ {ip_list} ] Press {index} to stop"
    
    def run_latest(config, image_class):
        if config["image_name"] in custom_env_routine.containers:
            for container in custom_env_routine.containers[config["image_name"]]:
                client.containers.get(container.id).remove(force=True)
            del custom_env_routine.containers[config["image_name"]]
            return
        env_list = []
        new_dict = {}
        for key, value in config["environment"].items():
            new_value = input(f"Enter value for {key} [default={value}]: ")
            if new_value == "":
                new_value = value
            env_list.append(f"{key}={new_value}")
            new_dict[f"{key}"] = f"{new_value}"
        print(env_list)
        print(new_dict)
        #container = image_class.run(env_list)
        version_list = docker_lib.image.list_version(client, image_class.name)
        latest = version_list[0]
        print(version_list)
        container = image_class.run(f"{image_class.name}:latest", env_list)


    def start():
        cer = custom_env_routine
        while True:
            cer.list_running_containers()
            print(cer.container_template(1, api_database.api_database.name))
            print(cer.container_template(2, api_server.api_server.name))
            print(cer.container_template(3, mosquitto_server.mosquitto_server.name))
            print(cer.container_template(4, auth_server.auth_server.name))
            print(cer.container_template(5, auth_database.auth_database.name))
            print(cer.container_template(6, oauth_server.oauth_server.name))
            print(cer.container_template(7, proxy_server.proxy_server.name))
            print("8. Exit")
            choice = input("Enter choice: ")
            if choice == "8":
                break
            if choice == "3":
                cer.run_latest(mosquitto_server_config, mosquitto_server.mosquitto_server)
                continue
            if choice == "4":
                cer.run_latest(auth_server_config, auth_server.auth_server)
                continue
            if choice == "5":
                cer.run_latest(auth_database.auth_database_config, auth_database.auth_database)
                continue
            if choice == "6":
                cer.run_latest(oauth_server_config, oauth_server.oauth_server)
                continue
            break