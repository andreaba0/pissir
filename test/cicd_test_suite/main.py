import docker
import os
import time
import os
import psycopg2
import re
from utility import Container
from runner.auth_backend.main import EntryPoint
from multiprocessing import Pool

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
            "user": "andrea",
            "password": "password",
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
        "POSTGRES_PASSWORD": "password",
        "POSTGRES_USER": "andrea",
        "POSTGRES_DB": "auth",
    }
    deleteAllContainersByTagName(imageName)
    baseImageName = imageName
    deleteAllOldImagesByTagName(baseImageName)
    currentPath = __location__
    dockerfilePath = currentPath + "/../../database/auth/"
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
    network = client.networks.get("test_auth_network")
    network.connect(
        container,
        ipv4_address=json["server"]["authDatabase"]["ip"]
    )
    ct = Container(container)
    ct.WaitTillRunning()
    ct.WaitRunningProcessOnPort("tcp", "0.0.0.0:5432", "LISTEN")
    ct.WaitIpAssignment(json["server"]["authDatabase"]["ip"], 16)
    ct.WaitPostgresStartingUp(json["server"]["authDatabase"]["ip"], json["server"]["authDatabase"]["exposed_port"])

def runAuthServerInstance():
    imageName = "test_auth_server"
    volumeName = "test_auth_server_log"
    envVariable = {
        "DOTNET_ENV_DATABASE_HOST": json["server"]["authDatabase"]["ip"],
        "DOTNET_ENV_DATABASE_PORT": json["server"]["authDatabase"]["exposed_port"],
        "DOTNET_ENV_DATABASE_USER": "andrea",
        "DOTNET_ENV_DATABASE_PASSWORD": "password",
        "DOTNET_ENV_DATABASE_NAME": "auth",
        "INITIAL_DATE": "01/01/2024 00:00:00",
    }
    deleteAllContainersByTagName(imageName)
    baseImageName = imageName
    deleteAllOldImagesByTagName(baseImageName)
    currentPath = __location__
    dockerfilePath = currentPath + "/../../backend"
    currentTimeStamp = str(int(time.time()))
    newImageName = baseImageName + ":" + currentTimeStamp
    #create volume if not exists
    volumes = client.volumes.list()
    if not any(volume.name == volumeName for volume in volumes):
        client.volumes.create(volumeName)

    image, build_log = client.images.build(
        path=dockerfilePath,
        dockerfile="Dockerfile.backend.auth", 
        tag=newImageName,
        labels={"image": baseImageName},
        rm=False
    )
    container = client.containers.run(
        newImageName,
        detach=True, 
        ports={'8000/tcp': 10200},
        environment=envVariable,
        name=baseImageName,
        auto_remove=True,
        labels={"image": baseImageName},
        volumes={volumeName: {"bind": "/shared_log", "mode": "rw"}}
    )
    ct = Container(container)
    ct.WaitTillRunning()
    ct.WaitRunningProcessOnPort("tcp", "0.0.0.0:8000", "LISTEN")
    network = client.networks.get("test_auth_network")
    network.connect(
        container,
        ipv4_address=json["server"]["authServer"]["ip"]
    )
    ct.WaitIpAssignment(json["server"]["authServer"]["ip"], 16)
    print(container.logs())

def runFakeOAuthProviderInstance():
    server_ip = json["server"]["fakeOAuthProvider"]["ip"]
    server_port = json["server"]["fakeOAuthProvider"]["exposed_port"]
    imageName = "test_fake_oauth_provider"
    envVariable = {
        "OAUTH_PROVIDER_PORT": server_port,
        "OAUTH_BIND_IP": "0.0.0.0",
        "OAUTH_PROVIDER_DOMAIN": "{ip}:{port}".format(ip=server_ip, port=server_port),
    }
    deleteAllContainersByTagName(imageName)
    baseImageName = imageName
    deleteAllOldImagesByTagName(baseImageName)
    currentPath = __location__
    dockerfilePath = currentPath + "/../../fake_oauth_server"
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
        environment=envVariable,
        name=baseImageName,
        auto_remove=True,
        labels={"image": baseImageName}
    )
    ct = Container(container)
    network = client.networks.get("test_auth_network")
    network.connect(
        container,
        ipv4_address=json["server"]["fakeOAuthProvider"]["ip"]
    )
    ct.WaitTillRunning()
    ct.WaitRunningProcessOnPort("tcp", f"0.0.0.0:{server_port}", "LISTEN")
    ct.WaitIpAssignment(json["server"]["fakeOAuthProvider"]["ip"], 16)
    print(container.logs())

def TestRoutine():
    EntryPoint(
        json["server"]["authDatabase"]["ip"],
        json["server"]["authDatabase"]["exposed_port"],
        json["server"]["authDatabase"]["database"],
        json["server"]["authDatabase"]["user"],
        json["server"]["authDatabase"]["password"],
        json["server"]["authServer"]["ip"],
        json["server"]["authServer"]["exposed_port"]
    )

def initAuthDatabase():
    backendConfig = json["server"]["fakeOAuthProvider"]
    provider_uri = f"http://{backendConfig['ip']}:{backendConfig['exposed_port']}/.well-known/openid-configuration"
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
    cur.execute('''
        insert into user_role (role_name) values('WA'), ('FA')
    ''')
    cur.execute('''
        insert into industry_sector(sector_name) values('WSP'), ('FAR')
    ''')
    cur.execute('''
        insert into registered_provider(provider_name, configuration_uri) values('test_provider', '{provider_uri}')
    '''.format(provider_uri=provider_uri))
    cur.execute('''
        insert into 
            allowed_audience(registered_provider, audience) 
        values
            ('test_provider', 'internal_workspace@appweb.andreabarchietto.it')
    ''')
    cur.close()
    conn.commit()
    conn.close()

def runDBInstanceWithSetup():
    runAuthDatabaseInstance()
    initAuthDatabase()

def main():
    choice = 0
    while True:
        print("1. Setup container workflow")
        print("2. Setup container network")
        print("3. Rebuild auth server instance")
        print("4. Run auth test routine")
        print("5. Exit")
        choice = int(input("Enter your choice: "))
        if choice<1 or choice>5:
            print("Choice must be in range {1,5}")
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
            runAuthServerInstance()
            continue
        if choice == 4:
            TestRoutine()
            continue
        if choice == 5:
            break

if __name__ == "__main__":
    main()