import docker
import re
import time
from utility.color_print import ColorPrint

class client:
    client = None
    def get_client():
        if client.client is None:
            client.client = docker.from_env()
        return client.client
    
    def get_latest_tag(image_name):
        client = client.get_client()
        # filter get tag json from images
        images = client.images.get(
            image_name,
            filters={"reference": [image_name]}
        )
        for image in images:
            print(image)

class image:
    def list_version(client, name):
        images = client.images.list(name)
        res_dict = {}
        match_pattern = r"^(?P<name>[a-z_]+):(?P<version>[0-9]+)$"
        for image in images:
            match = re.match(match_pattern, image.tags[0])
            if match is None:
                continue
            if match.group("name") != name:
                continue
            int_version = int(match.group("version"))
            res_dict[int_version] = True
        res_list = []
        for key in sorted(res_dict.keys(), reverse=True):
            res_list.append(f"{name}:{key}")
        return res_list


class Container:
    def __init__(self, container):
        self.container = container
    def WaitForStringInLogs(self, string, timeout=30):
        currentTime = 0
        while True:
            currentTime += 1
            if currentTime > timeout:
                raise Exception("Time for string to appear in logs timed out")
            logs = self.container.logs().decode("utf-8")
            
            # search for string with regex
            regexString = f"^{string}$"
            regexIter = re.finditer(regexString, logs, flags=re.MULTILINE)
            lastOccurance = None
            for match in regexIter:
                lastOccurance = match
            if lastOccurance is None:
                time.sleep(1)
                ColorPrint.print(0, [
                    ("GRAY", f"Searching for string {string} in {self.container.name}: "),
                    ("YELLOW", "Keep searching")
                ])
                time.sleep(1)
                continue
            else:
                ColorPrint.print(0, [
                    ("GRAY", f"Searching for string {string} in {self.container.name}: "),
                    ("GREEN", "Found")
                ])
                return


    # The following function connects to a container and checks if a process is running on a specific port
    # This method is required to test that the process running in the container is effectively listening for incoming connections
    # In this case package net-tools is required to run netstat
    def WaitRunningProcessOnPort(self, proto, local_address, state, timeout=30):
        currentTime = 0
        while True:
            ColorPrint.print(0, [
                ("GRAY", f"State of process {proto} {local_address} {state}: "),
                ("YELLOW", "Waiting for process to start")
            ])
            currentTime += 1
            if currentTime > timeout:
                break
            (code, out) = self.container.exec_run("netstat -antu", stdout=True)
            asciiOut = out.decode("ascii")
            regexString = r"^(?P<proto>((tcp)|(udp)))( )+[0-9]+( )+[0-9]+( )+(?P<local_address>([0-9.:]+))( )+(?P<foreign_address>[0-9.:*]+)( )+(?P<state>[A-Z]+)( )+$"
            regexIter = re.finditer(regexString, asciiOut, flags=re.MULTILINE)
            for i, match in enumerate(regexIter):
                if match.group("local_address") != local_address:
                    continue
                if match.group("state") != state:
                    continue
                if match.group("proto") != proto:
                    continue
                ColorPrint.print(0, [
                    ("GRAY", f"State of process {proto} {local_address} {state}: "),
                    ("GREEN", "Running")
                ])
                return
            time.sleep(1)
        raise Exception("Time for process to start timed out")
    
    # The following function connects to a container and checks if an IP address has been assigned to a specific interface
    # This method is required to test that the network configuration of the container is correct
    def WaitIpAssignment(self, ip, netmask, timeout=30):
        currentTime = 0
        while True:
            ColorPrint.print(0, [
                ("GRAY", f"State of assignment {ip}/{netmask}: "),
                ("YELLOW", "Waiting for IP assignment")
            ])
            currentTime += 1
            if currentTime > timeout:
                break
            (code, out) = self.container.exec_run("ip addr", stdout=True)
            asciiOut = out.decode("ascii")
            regexString = r"^[0-9]+: (?P<if_name>([a-zA-Z0-9]|@)+):(.*?)state (?P<state>(UP|DOWN|UNKNOWN)).+\n(.*?)\n(.*?)[a-z0-9]+ (?P<ip>[0-9.?]+\/[0-9]+)( brd (?P<broadcast>[0-9.?]+))?"
            regexIter = re.finditer(regexString, asciiOut, flags=re.MULTILINE)
            for match in regexIter:
                if match.group("ip") != f"{ip}/{netmask}":
                    continue
                if match.group("state") != "UP":
                    continue
                ColorPrint.print(0, [
                    ("GRAY", f"State of assignment {ip}/{netmask}: "),
                    ("GREEN", "Assigned")
                ])
                return
            time.sleep(1)
        raise Exception("Time for ip assignment timed out")
    
    def WaitPostgresStartingUp(self, ip, port, timeout=30):
        currentTime = 0
        while True:
            ColorPrint.print(0, [
                ("GRAY", f"State of postgres {ip}:{port}: "),
                ("YELLOW", "Waiting for postgres to start")
            ])
            currentTime += 1
            if currentTime > timeout:
                break
            (code, out) = self.container.exec_run(f"pg_isready -h {ip} -p {port}", stdout=True)
            asciiOut = out.decode("ascii")
            regexString = r"^(?P<address>(.*?)) - (?P<status>(accepting connections)|(no response)|(rejecting connections))$"
            regexIter = re.finditer(regexString, asciiOut, flags=re.MULTILINE)
            for match in regexIter:
                if match.group("address") != f"{ip}:{port}":
                    continue
                if match.group("status") != "accepting connections":
                    continue
                ColorPrint.print(0, [
                    ("GRAY", f"State of postgres {ip}:{port}: "),
                    ("GREEN", "Running")
                ])
                return
            time.sleep(1)
        raise Exception("Time for postgres to start timed out")
    
    def WaitTillRunning(self, timeout=30):
        currentTime = 0
        while(self.container.status != "running"):
            currentTime += 1
            if currentTime > timeout:
                raise Exception("Time for container to start timed out")
            time.sleep(1)
            ColorPrint.print(0, [
                ("GRAY", f"State of container {self.container.name}: "),
                ("YELLOW", "Waiting for container to start")
            ])
            self.container.reload()
        ColorPrint.print(0, [
            ("GRAY", f"State of container {self.container.name}: "),
            ("GREEN", "Running")
        ])

class AuthBackendContainer(Container):
    def __init__(self, container):
        super().__init__(container)
    
    def WaitTillKeysAreDownloaded(self, timeout=30):
        currentTime = 0
        while True:
            currentTime += 1
            if currentTime > timeout:
                raise Exception("Time for string to appear in logs timed out")
            logs = self.container.logs().decode("utf-8")
            regexString = r"^Query RSA keys from database: (?P<state>(SUCCESS))$"
            regexIter = re.finditer(regexString, logs, flags=re.MULTILINE)
            lastOccurance = None
            for match in regexIter:
                if match.group("state") == "SUCCESS":
                    lastOccurance = match
            if lastOccurance is None:
                time.sleep(1)
                ColorPrint.print(0, [
                    ("GRAY", f"Searching for keys state in {self.container.name}: "),
                    ("YELLOW", "Keep searching")
                ])
                time.sleep(1)
                continue
            ColorPrint.print(0, [
                ("GRAY", f"Searching for keys state in {self.container.name}: "),
                ("GREEN", "Found")
            ])
            return


class image_name:
    def validate(name):
        regexString = r"^(?P<project>(appweb|pissir))_(?P<name>[a-z0-9_]+)$"
        regexIter = re.finditer(regexString, name)
        for match in regexIter:
            return True
        return False
    
            
            
