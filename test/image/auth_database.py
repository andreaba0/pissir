from config.auth_database_server import auth_database_config
import os
import docker
import time

client = docker.from_env()

class auth_database:
    name = auth_database_config["image_name"]

    def build():
        baseImageName = imageName
        currentPath = __location__
        dockerfilePath = os.path.join(currentPath, auth_database_config["dockerfile_path"])
        currentTimeStamp = str(int(time.time()))
        newImageName = baseImageName + ":" + currentTimeStamp
        print(dockerfilePath)
        image, build_log = client.images.build(
            path=dockerfilePath,
            dockerfile=auth_database_config["dockerfile_name"], 
            tag=newImageName,
            labels={"image": baseImageName},
            rm=True
        )
    
    def stop():
        return

    def run_latest():
        return
    
    def converge_to_running():
        return