import ipaddress
from dotenv import load_dotenv
import os

class address_manager:
    network_name = "pissir_network"
    address_space = None #eg. 172.16.10.0/26
    index = 1
    port_space = 10150

    def get_address():
        if address_manager.address_space is None:
            load_dotenv()
            ip_address_space = os.getenv("IP_ADDRESS_SPACE")
            if ip_address_space is None:
                raise Exception("IP_ADDRESS_SPACE environment variable is not set")
            address_space = ipaddress.ip_network(ip_address_space)
            if address_space.is_private is False:
                raise Exception("IP_ADDRESS_SPACE must be a private network")
            if address_space.num_addresses < 16:
                raise Exception("IP_ADDRESS_SPACE must be at least /28")
            address_manager.address_space = address_space
        address_space = address_manager.address_space
        index = address_manager.index
        if address_space is None:
            raise Exception("Address space not set")
        if index >= address_space.num_addresses:
            raise Exception("Address space exhausted")
        address = address_space.network_address + index
        address_manager.index += 1
        return address
    
    def reset_address():
        address_manager.index = 1
    
    def get_port():
        port = address_manager.port_space
        address_manager.port_space += 1
        return port
    
    def reset_port():
        address_manager.port_space = 10150
    