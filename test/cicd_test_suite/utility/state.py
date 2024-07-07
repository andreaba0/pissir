from cicd_test_suite.utility.docker_lib import client
from enum import Enum
from config.address_manager import address_manager
import docker


client = client.get_client()

class StateManager:
    def converge(blocks):
        for obj in blocks:
            if type(obj) is not Block:
                raise Exception("Argument should be a list of Block objects")
        
        ls_raw = client.containers.list(all=True)
        containers = []
        for block in blocks:
            if block.state == State.RUNNING:
                containers.append(None)
                continue
            for container in ls_raw:
                if container.labels.get("com.pissir.env") != "testing":
                    continue
                if container.labels.get("com.pissir.role") == block.config.get("image_name"):
                    client.containers.get(container.id).remove(force=True)
            container = None
            if block.state == State.NEW:
                container = block.image.run(f"{block.image.name}:latest", block.config["environment"])
                NetworkState.connect(block.config["network"]["name"], block.config["network"]["ip"], container)
            containers.append(container) 
        return containers

class Block:
    def __init__(self, image, config, state):
        self.image = image
        self.config = config
        self.state = state

class State(Enum):
    RUNNING = 1
    NEW = 2
    CLEAR = 3


class NetworkState:
    def connect(network_name, ip, container):
        if network_name != address_manager.network_name:
            raise Exception("Network name should be same as address_manager")
        
        network = None
        try:
            network = client.networks.get(network_name)
            found = False
            for subnet in network.attrs["IPAM"]["Config"]:
                if subnet["Subnet"] == f"{address_manager.get_address_space()}":
                    found = True
                    break
            if not found:
                raise Exception("Subnet not found in network")
        except docker.errors.NotFound:
            client.networks.create(
                name=network_name, 
                driver="bridge",
                ipam=docker.types.IPAMConfig(
                    pool_configs=[docker.types.IPAMPool(
                        subnet=f"{address_manager.get_address_space()}"
                    )]
                )
            )
            network = client.networks.get(network_name)
        network.connect(container, ipv4_address=f"{ip}")