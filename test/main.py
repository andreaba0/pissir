import docker
import os
import time
from utility import Assert, Group
from auth import SignUpEntryPoint
import os
import psycopg2
import re

__location__ = os.path.realpath(
    os.path.join(os.getcwd(), os.path.dirname(__file__)))

client = docker.from_env()

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
            "host_port": 10201,
            "user": "postgres",
            "password": "postgres",
            "database": "auth"
        },
    },
    "network": {
        "name": "test_auth_network",
        "ip_range": "172.10.0.0/16"
    }
}

def testContainerList():
    for container in client.containers.list(all=True):
        container.stop()
        container.wait()
        container.remove(force=True)

def TagIsInList(tagName, tagList):
    for tag in tagList:
        name = tag.split(':')[0]
        if name == tagName:
            return True
    return False

def deleteAllContainersByTagName(tagName):
    toRemove = []
    for container in client.containers.list(all=True):
        if TagIsInList(tagName, container.image.tags):
            toRemove.append(container)
    for container in toRemove:
        if container.status != "exited":
            container.stop()
            container.wait()
    #prune container wheere label is image=tagName
    client.containers.prune(filters={"label": "image=" + tagName})

def deleteAllOldImagesByTagName(tagName):
    images = client.images.list()
    for image in images:
        for tag in image.tags:
            name = tag.split(':')[0]
            if name == tagName:
                image.remove()

def setupBridgeAuthNetwork():
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
    deleteAllContainersByTagName(imageName)
    baseImageName = imageName
    deleteAllOldImagesByTagName(baseImageName)
    currentPath = os.getcwd()
    dockerfilePath = currentPath + "/../database/auth/"
    currentTimeStamp = str(int(time.time()))
    newImageName = baseImageName + ":" + currentTimeStamp
    print(dockerfilePath)
    image, build_log = client.images.build(
        path=dockerfilePath,
        dockerfile="Dockerfile", 
        tag=newImageName,
        labels={"image": baseImageName},
        rm=True
    )
    container = client.containers.run(
        newImageName, 
        detach=True, 
        ports={'5432/tcp': 10201},
        environment=envVariable,
        name=baseImageName,
        auto_remove=True,
        labels={"image": baseImageName}
    )
    while(container.status != "running"):
        time.sleep(1)
        container.reload()
    network = client.networks.get("test_auth_network")
    network.connect(
        container,
        ipv4_address=json["server"]["authDatabase"]["ip"]
    )
    postgresIsRunning = False
    while not postgresIsRunning:
        (code, out) = container.exec_run("netstat -an", stdout=True)
        asciiOut = out.decode("ascii")
        regexString = r"^(?P<proto>((tcp)|(udp)))( )+[0-9]+( )+[0-9]+( )+(?P<local_address>([0-9.:]+))( )+(?P<foreign_address>[0-9.:*]+)( )+(?P<state>[A-Z]+)( )+$"
        regexIter = re.finditer(regexString, asciiOut, flags=re.MULTILINE)
        for i, match in enumerate(regexIter):
            print(f"Iteration number {i}")
            print(match.group("proto"))
            print(match.group("local_address"))
            print(match.group("foreign_address"))
            print(match.group("state"))
            if match.group("local_address") == "0.0.0.0:5432":
                if match.group("state") == "LISTEN":
                    postgresIsRunning = True
                    break
        time.sleep(1)
    privateIpAssigned = False
    while not privateIpAssigned:
        (code, out) = container.exec_run("ip addr", stdout=True)
        asciiOut = out.decode("ascii")
        regexString = r"^[0-9]+: (?P<if_name>[a-zA-Z0-9]+(@[a-z-A-Z0-9]+)?:)(.*?)state (?P<state>([A-Z]+)) +\n(.* ?)\n( +)[a-z]+ (?P<ip>[0-9.?]+\/[0-9]+) brd (?P<broadcast>[0-9.?]+)"
        regexIter = re.finditer(regexString, asciiOut, flags=re.MULTILINE)
        for match in regexIter:
            print(match.group("if_name"))
            print(match.group("state"))
            print(match.group("ip"))
            print(match.group("broadcast"))
            if match.group("ip") == json["server"]["authDatabase"]["ip"] + "/16":
                if match.group("state") == "UP":
                    privateIpAssigned = True
                    break
        time.sleep(1)

def runAuthServerInstance():
    imageName = "test_auth_server"
    envVariable = {
        "DOTNET_ENV_DATABASE_HOST": json["server"]["authDatabase"]["ip"],
        "DOTNET_ENV_DATABASE_PORT": json["server"]["authDatabase"]["exposed_port"],
        "DOTNET_ENV_DATABASE_USER": "postgres",
        "DOTNET_ENV_DATABASE_PASSWORD": "postgres",
        "DOTNET_ENV_DATABASE_NAME": "auth",
    }
    deleteAllContainersByTagName(imageName)
    baseImageName = imageName
    deleteAllOldImagesByTagName(baseImageName)
    currentPath = os.getcwd()
    dockerfilePath = currentPath + "/../backend"
    currentTimeStamp = str(int(time.time()))
    newImageName = baseImageName + ":" + currentTimeStamp
    image, build_log = client.images.build(
        path=dockerfilePath,
        dockerfile="Dockerfile.backend.auth", 
        tag=newImageName,
        labels={"image": baseImageName},
        rm=True
    )
    container = client.containers.run(
        newImageName,
        detach=True, 
        ports={'8000/tcp': 10200},
        environment=envVariable,
        name=baseImageName,
        auto_remove=True,
        labels={"image": baseImageName}
    )
    network = client.networks.get("test_auth_network")
    network.connect(
        container,
        ipv4_address=json["server"]["authServer"]["ip"]
    )
    print(container.logs())

def runFakeOAuthProviderInstance():
    imageName = "test_fake_oauth_provider"
    deleteAllContainersByTagName(imageName)
    baseImageName = imageName
    deleteAllOldImagesByTagName(baseImageName)
    currentPath = os.getcwd()
    dockerfilePath = currentPath + "/../oauth_redirect"
    currentTimeStamp = str(int(time.time()))
    newImageName = baseImageName + ":" + currentTimeStamp
    image, build_log = client.images.build(
        path=dockerfilePath,
        dockerfile="Dockerfile", 
        tag=newImageName,
        labels={"image": baseImageName},
        rm=True
    )
    container = client.containers.run(
        newImageName, 
        detach=True, 
        ports={'8000/tcp': 10203},
        name=baseImageName,
        auto_remove=True,
        labels={"image": baseImageName}
    )
    network = client.networks.get("test_auth_network")
    network.connect(
        container,
        ipv4_address=json["server"]["fakeOAuthProvider"]["ip"]
    )
    print(container.logs())

def TestRoutine():
    SignUpEntryPoint(
        json["server"]["authDatabase"]["ip"],
        json["server"]["authDatabase"]["exposed_port"],
        json["server"]["authDatabase"]["database"],
        json["server"]["authDatabase"]["user"],
        json["server"]["authDatabase"]["password"],
        json["server"]["authServer"]["ip"],
        json["server"]["authServer"]["exposed_port"]
    )

def initAuthDatabase():
    #connect to database and run auth/init_default.sql file
    with open(os.path.join(__location__, 'auth/init_default.sql'), "r") as file:
        setupSql = file.read()
    backendConfig = json["server"]["fakeOAuthProvider"]
    setupSql = setupSql.replace("<provider_uri>", f"http://{backendConfig['ip']}:{backendConfig['exposed_port']}/.well-known/openid-configuration")
    #connect to database
    databaseConfig = json["server"]["authDatabase"]
    conn = psycopg2.connect(
        host=databaseConfig["ip"],
        port=databaseConfig["exposed_port"],
        database=databaseConfig["database"],
        user=databaseConfig["user"],
        password=databaseConfig["password"]
    )
    cur = conn.cursor()
    cur.execute(setupSql)
    conn.commit()
    cur.close()
    conn.close()

def main():

    #testContainerList()
    #return
    choice = 0
    while True:
        print("1. Setup container workflow")
        print("2. Setup container network")
        print("3. Run auth test routine")
        print("4. Exit")
        choice = int(input("Enter your choice: "))
        if choice<1 or choice>4:
            print("Choice must be in range {1,4}")
            continue
        if choice == 1:
            runAuthDatabaseInstance()
            initAuthDatabase()
            runFakeOAuthProviderInstance()
            runAuthServerInstance()
            continue
        if choice == 2:
            setupBridgeAuthNetwork()
            continue
        if choice == 3:
            TestRoutine()
            continue
        if choice == 4:
            break

if __name__ == "__main__":
    main()