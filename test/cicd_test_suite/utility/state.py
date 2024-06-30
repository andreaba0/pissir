from cicd_test_suite.utility.docker_lib import client
from enum import Enum



client = client.get_client()

class StateManager:
    def tierdown():
        #get list of running containers
        ls_raw = client.containers.list(all=True)
        for container in ls_raw:
            return
    def converge(blocks):
        for obj in blocks:
            if type(obj) is not Block:
                raise Exception("Arguments should be an instance of Block class")
        
        #get list of running containers
        ls_raw = client.containers.list(all=True)
        for container in ls_raw:
            for tag in container.image.tags:
                if tag 
                print(tag)

class Block:
    def __init__(self, image, config, state):
        self.image = image
        self.config = config
        self.state = state

class State(Enum):
    RUNNING = 1
    NEW = 2
    CLEAR = 3