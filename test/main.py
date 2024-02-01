import docker
import os
import time
from utility import Assert, Group

#json object
json = {
    "server": {
        "fakeOAuthProvider": {
            "ip": "172.10.0.4",
            "exposed_port": 8000,
            "host_port": 10203
        },
        "authServer": {
            "ip": "172.10.0.2",
            "exposed_port": 8000,
            "host_port": 10200
        },
        "authDatabase": {
            "ip": "172.10.0.3",
            "exposed_port": 5432,
            "host_port": 10201
        },
    },
    "network": {
        "name": "test_auth_network",
        "ip_range": "172.10.0.0/16"
    }
}

def setupBridgeAuthNetwork():
    client = docker.from_env()
    networks = client.networks.list()
    for network in networks:
        if network.name == "test_auth_network":
            print("Network already exists")
            return
    #create network and assign ip range 172.10.0.0/16
    client.networks.create(
        "test_auth_network", 
        driver="bridge",
        ipam=docker.types.IPAMConfig(
            pool_configs=[docker.types.IPAMPool(
                subnet='172.10.0.0/16',
            )]
        )
    )

def runAuthDatabaseInstance():
    imageName = "test_auth_database"
    envVariable = {
        "POSTGRES_PASSWORD": "postgres",
        "POSTGRES_USER": "postgres",
        "POSTGRES_DB": "auth",
    }
    client=docker.from_env()
    runningContainers = client.containers.list()
    for container in runningContainers:
        for tag in container.image.tags:
            name = tag.split(':')[0]
            if name == imageName:
                print("Auth database server is already running")
                return
    currentPath = os.getcwd()
    dockerfilePath = currentPath + "/../database/auth/"
    print(dockerfilePath)
    image, build_log = client.images.build(path=dockerfilePath, tag="test_auth_database")
    container = client.containers.run(
        image, 
        detach=True, 
        ports={'5432/tcp': 10201},
        environment=envVariable,
    )
    network = client.networks.get("test_auth_network")
    network.connect(
        container,
        ipv4_address="172.10.0.3"
    )
    print(container.logs())

def runAuthServerInstance():
    imageName = "test_auth_server"
    envVariable = {
        "DOTNET_ENV_DATABASE_HOST": "172.10.0.3",
        "DOTNET_ENV_DATABASE_PORT": "5432",
        "DOTNET_ENV_DATABASE_USER": "postgres",
        "DOTNET_ENV_DATABASE_PASSWORD": "postgres",
        "DOTNET_ENV_DATABASE_NAME": "auth",
    }
    client=docker.from_env()
    runningContainers = client.containers.list()
    for container in runningContainers:
        for tag in container.image.tags:
            name = tag.split(':')[0]
            if name == imageName:
                container.stop()
                container.remove()
    baseImageName = "test_auth_server"
    # delete old image
    images = client.images.list()
    for image in images:
        for tag in image.tags:
            name = tag.split(':')[0]
            if name == baseImageName:
                image.remove()
    currentPath = os.getcwd()
    dockerfilePath = currentPath + "/../backend"
    currentTimeStamp = str(int(time.time()))
    newImageName = baseImageName + ":" + currentTimeStamp
    image, build_log = client.images.build(
        path=dockerfilePath,
        dockerfile="Dockerfile.backend.auth", 
        tag=newImageName
    )
    container = client.containers.run(
        image, 
        detach=True, 
        ports={'8000/tcp': 10200},
        environment=envVariable
    )
    network = client.networks.get("test_auth_network")
    network.connect(
        container,
        ipv4_address="172.10.0.2"
    )
    print(container.logs())

def runFakeOAuthProviderInstance():
    imageName = "test_fake_oauth_provider"
    client=docker.from_env()
    runningContainers = client.containers.list()
    for container in runningContainers:
        for tag in container.image.tags:
            name = tag.split(':')[0]
            if name == imageName:
                container.stop()
                container.remove()
    baseImageName = "test_fake_oauth_provider"
    # delete old image
    images = client.images.list()
    for image in images:
        for tag in image.tags:
            name = tag.split(':')[0]
            if name == baseImageName:
                image.remove()
    currentPath = os.getcwd()
    dockerfilePath = currentPath + "/../oauth_redirect"
    currentTimeStamp = str(int(time.time()))
    newImageName = baseImageName + ":" + currentTimeStamp
    image, build_log = client.images.build(
        path=dockerfilePath,
        dockerfile="Dockerfile", 
        tag=newImageName
    )
    container = client.containers.run(
        image, 
        detach=True, 
        ports={'8000/tcp': 10203},
    )
    network = client.networks.get("test_auth_network")
    network.connect(
        container,
        ipv4_address="172.10.0.4"
    )
    print(container.logs())

def main():

    client = docker.from_env()
    choice = 0
    while True:
        print("1. Run auth database")
        print("2. Run auth server")
        print("3. Run api database")
        print("4. Run api server")
        print("5. Run OAuth fake server")
        print("6. Setup bridge network")
        print("7. Exit")
        choice = int(input("Enter your choice: "))
        if choice<1 or choice>7:
            print("Choice must be in range {1,7}")
            continue
        if choice == 1:
            runAuthDatabaseInstance()
        if choice == 2:
            runAuthServerInstance()
        if choice == 5:
            runFakeOAuthProviderInstance()
        if choice == 6:
            setupBridgeAuthNetwork()
        if choice == 7:
            break

if __name__ == "__main__":
    main()