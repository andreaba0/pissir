import docker

client = docker.from_env()

class pissir_network:
    name = "pissir_network"

    def create():
        networks = client.networks.list()
        for network in networks:
            if network.name == pissir_network.name:
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
        return
    
    def drop():
        return
    
    def inspect():
        return
        