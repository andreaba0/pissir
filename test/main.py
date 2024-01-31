import docker
import os

def setupBridgeAuthNetwork():
    client = docker.from_env()
    networks = client.networks.list()
    for network in networks:
        if network.name == "test_auth_network":
            print("Network already exists")
            return
    client.networks.create("test_auth_network", driver="bridge")

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
        ports={'5432/tcp': 5432},
        environment=envVariable,
        network="test_auth_network"
    )
    print(container.logs())

def runAuthServerInstance():
    imageName = "test_auth_server"
    envVariable = {
        "DOTNET_ENV_DATABASE_HOST": "localhost",
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
                print("Auth server is already running")
                return
    currentPath = os.getcwd()
    dockerfilePath = currentPath + "/../backend"
    print(dockerfilePath)
    image, build_log = client.images.build(
        path=dockerfilePath,
        dockerfile="Dockerfile.backend.auth", 
        tag="test_auth_server"
    )
    container = client.containers.run(
        image, 
        detach=True, 
        ports={'5000/tcp': 5000},
        environment=envVariable,
        network="test_auth_network"
    )
    print(container.logs())

def main():
    client = docker.from_env()
    setupBridgeAuthNetwork()
    choice = 0
    while True:
        print("1. Run auth database")
        print("2. Run auth server")
        print("3. Run api database")
        print("4. Run api server")
        print("5. Exit")
        choice = int(input("Enter your choice: "))
        if choice<1 or choice>5:
            print("Choice must be in range {1,5}")
            continue
        if choice == 1:
            runAuthDatabaseInstance()
        if choice == 2:
            runAuthServerInstance()
        if choice == 5:
            break

if __name__ == "__main__":
    main()