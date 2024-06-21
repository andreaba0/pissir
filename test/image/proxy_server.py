import os
import time

from cicd_test_suite.utility import docker_lib

from config.proxy_server import proxy_server_config

from dotenv import load_dotenv

class proxy_server:
    name = proxy_server_config["image_name"]

    def build():
        load_dotenv()
        client = docker_lib.client.get_client()
        basePath = os.getenv("BASE_PATH")
        baseImageName = proxy_server_config["image_name"]
        currentPath = basePath
        dockerfilePath = os.path.join(currentPath, proxy_server_config["dockerfile_path"])
        currentTimeStamp = str(int(time.time()))
        newImageName = baseImageName + ":" + currentTimeStamp
        print(dockerfilePath)
        image, build_log = client.images.build(
            path=dockerfilePath,
            dockerfile=proxy_server_config["dockerfile_name"], 
            tag=newImageName,
            labels={"image": baseImageName},
            rm=True
        )
    
    def stop():
        return

    def run_latest(env_vars):
        return
    
    def converge_to_running():
        return