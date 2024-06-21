from image import api_database
from image import api_server
from image import mosquitto_server
from image import auth_server
from image import auth_database
from image import oauth_server
from image import proxy_server

from cicd_test_suite.utility import docker_lib

client = docker_lib.client.get_client()

from config.auth_server import auth_server_config

class custom_env_routine:

    images = {}

    def list_running_containers():
        ls_raw = client.containers.list(
            all=False
        )
        # return object: [{id, name, image, status, bridge_ip}]
        # to get bridge_ip it is required to run inspect on container for each container
        ls = []
        custom_env_routine.images = {}
        for container in ls_raw:
            container_data = client.containers.get(container.id)
            image_name = container_data.attrs["Config"]["Image"].split(":")[0]
            #if docker_lib.image_name.validate(image_name) == False:
             #   continue
            bridge_ip = container_data.attrs["NetworkSettings"]["Networks"]["bridge"]["IPAddress"]
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
        img = custom_env_routine.images.get(container_name)
        if img is None:
            return f"{index}. [ offline ] {container_name} Press {index} to run"
        ip_list = ", ".join(img["bridge_ip"])
        return f"{index}. [ online ] {container_name} [{ip_list}] Press {index} to stop"
        cer = custom_env_routine
        return f"{index}. {cer.get_container_stat(container_name)}: {container_name} {cer.get_container_ip(container_name)}"

    def auth_server():
        # display keys in environment dictionary of auth_server_config in KEY=VALUE format
        for key, value in auth_server_config["environment"].items():
            print(f"{key}={value}")
    
    def run_container(image_name):
        return
    
    def stop_container(image_name):
        return
    
    def run_latest(config, image_class):
        env_list = []
        for key, value in config["environment"].items():
            new_value = input(f"Enter value for {key} [default={value}]: ")
            if new_value == "":
                new_value = value
            env_list.append(f"{key}={new_value}")
        container = image_class.run(env_list)


    def start():
        cer = custom_env_routine
        cer.auth_server()
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
            if choice == "4":
                cer.run_latest(auth_server_config, auth_server.auth_server)
                continue
            break