from image import api_database
from image import api_server
from image import mosquitto_server
from image import auth_server
from image import auth_database
from image import oauth_server
from image import proxy_server

from config.auth_server import auth_server_config

class custom_env_routine:

    def get_container_stat(container_name):
        return "[status: online]"
    
    def get_container_ip(container_name):
        return f"with ip address: {None}"
    
    def container_template(index, container_name):
        cer = custom_env_routine
        return f"{index}. {cer.get_container_stat(container_name)}: {container_name} {cer.get_container_ip(container_name)}"

    def auth_server():
        # display keys in environment dictionary of auth_server_config in KEY=VALUE format
        for key, value in auth_server_config["environment"].items():
            print(f"{key}={value}")

    def start():
        cer = custom_env_routine
        cer.auth_server()
        while True:
            print(cer.container_template(1, api_database.api_database.name))
            print(cer.container_template(2, api_server.api_server.name))
            print(cer.container_template(3, mosquitto_server.mosquitto_server.name))
            print(cer.container_template(4, auth_server.auth_server.name))
            print(cer.container_template(5, auth_database.auth_database.name))
            print(cer.container_template(6, oauth_server.oauth_server.name))
            print(cer.container_template(7, proxy_server.proxy_server.name))
            print("8. Exit")
            choice = int(input("Enter choice: "))
            if choice == 8:
                break
            break