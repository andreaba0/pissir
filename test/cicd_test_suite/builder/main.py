from datetime import datetime
import re

from config import auth_database_server
from config import api_database_server
from config import api_server
from config import mosquitto_server
from config import auth_server
from config import oauth_server
from config import proxy_server
from image import proxy_server
from image import auth_database
from image import api_database
from image import auth_server
from cicd_test_suite.utility import docker_lib
client = docker_lib.client.get_client()

class image_builder:

    image_version = {}

    def parseTimeStringToDateTime(timestring):
        pattern = r'^(?P<year>\d{4})-(?P<month>[0-9]{2})-(?P<day>[0-9]{2})T(?P<hour>[0-9]{2}):(?P<minute>[0-9]{2}):(?P<second>[0-9]{2}).\d+(?P<timezone>\+\d{2}:\d{2})$'
        ismatch = re.match(pattern, timestring)
        if ismatch == None:
            raise Exception("Invalid date format")
        keydict = ismatch.groupdict()
        dt = datetime(
            year=int(keydict["year"]),
            month=int(keydict["month"]),
            day=int(keydict["day"]),
            hour=int(keydict["hour"]),
            minute=int(keydict["minute"]),
            second=int(keydict["second"]),
            microsecond=0,
            tzinfo=None
        )
        return dt
    
    def get_version_info():
            images = client.images.list()
            latest_version = None
            versions = 0
            for image in images:
                if image.labels.get("image") == None or docker_lib.image_name.validate(image.labels.get("image")) == False:
                    continue
                for key, value in image.attrs.items():
                    print(f"{key}: {value}")
                print("\n\n\n\n\n\n\n\n")
                current_image = image.labels.get("image")
                if image_builder.image_version.get(current_image) == None:
                    image_builder.image_version[current_image] = {}
                versions = len(image.tags)
                for tag in image.tags:
                    date = image.attrs["Metadata"]["LastTagTime"]
                    if latest_version == None:
                        latest_version = date
                    elif image_builder.parseTimeStringToDateTime(date) > image_builder.parseTimeStringToDateTime(latest_version):
                        latest_version = date
                image_builder.image_version[current_image]["latest_version"] = latest_version
                image_builder.image_version[current_image]["versions"] = versions

    def create_string(index, image_name):
        if image_builder.image_version.get(image_name) == None:
            return f"{index}. [ 0 versions ] {image_name} Press {index} to build"
        # parsed_date should be in format: DD-MM-YYYY HH:MM:SS
        timestring = image_builder.image_version[image_name]["latest_version"]
        parsed_date = image_builder.parseTimeStringToDateTime(timestring).strftime("%d-%m-%Y %H:%M:%S")
        return f"{index}. [ {image_builder.image_version[image_name]['versions']} versions ][ latest: {parsed_date} ] {image_name} Press {index} to build"
    
    def automate_build_steps(config, builder):
        builder.build()


    def main():
        while True:
            image_builder.get_version_info()
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
            if choice == "1":
                image_builder.automate_build_steps(auth_database_server.auth_database_config, auth_database.auth_database)
                continue
            if choice == "2":
                image_builder.automate_build_steps(api_database_server.api_database_config, api_database.api_database)
                continue
            if choice == "5":
                image_builder.automate_build_steps(auth_server.auth_server_config, auth_server.auth_server)
                continue
            if choice == "7":
                proxy_server.proxy_server.build()
                continue