from config import auth_database_server
from config import api_database_server
from config import api_server
from config import mosquitto_server
from config import auth_server
from config import oauth_server
from config import proxy_server
from image import proxy_server
from cicd_test_suite.utility import docker_lib
client = docker_lib.client.get_client()

class image_builder:

    image_version = {}
    
    def get_version_info():
            images = client.images.list()
            latest_version = None
            versions = 0
            for image in images:
                print(image.attrs["Created"])
                if image.labels.get("image") == None or docker_lib.image_name.validate(image.labels.get("image")) == False:
                    continue
                current_image = image.labels.get("image")
                if image_builder.image_version.get(current_image) == None:
                    image_builder.image_version[current_image] = {}
                versions = len(image.tags)
                for tag in image.tags:
                    date = int(tag.split(":")[1])
                    if latest_version == None:
                        latest_version = image.attrs["Created"]
                    elif date > latest_version:
                        latest_version = image.attrs["Created"]
                image_builder.image_version[current_image]["latest_version"] = latest_version
                image_builder.image_version[current_image]["versions"] = versions

    def create_string(index, image_name):
        if image_builder.image_version.get(image_name) == None:
            return f"{index}. [ 0 versions ] {image_name} Press {index} to build"
        # parsed_date should be in format: DD-MM-YYYY HH:MM:SS
        parsed_date = image_builder.image_version[image_name]["latest_version"]
        return f"{index}. [ {image_builder.image_version[image_name]['versions']} versions ][ latest: {parsed_date} ] {image_name} Press {index} to build"
    
    def main():
        image_builder.get_version_info()
        while True:
            print(image_builder.create_string(1, auth_database_server.auth_database_config["image_name"]))
            print(image_builder.create_string(2, api_database_server.api_database_config["image_name"]))
            print(image_builder.create_string(3, api_server.api_server_config["image_name"]))
            print(image_builder.create_string(4, mosquitto_server.mosquitto_server_config["image_name"]))
            print(image_builder.create_string(5, auth_server.auth_server_config["image_name"]))
            print(image_builder.create_string(6, oauth_server.oauth_server_config["image_name"]))
            print(image_builder.create_string(7, proxy_server.proxy_server_config["image_name"]))
            print("8. Exit")
            choice = input("Enter choice: ")
            if choice == "8":
                break
            if choice == "7":
                proxy_server.proxy_server.build()
                continue