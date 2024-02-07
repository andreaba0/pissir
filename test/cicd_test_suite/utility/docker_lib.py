import docker
import re
import time
from utility.color_print import ColorPrint

class Container:
    def __init__(self, container):
        self.container = container
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
            (code, out) = self.container.exec_run("netstat -an", stdout=True)
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
    def WaitIpAssignment(self, ip, netmask, timeout=30):
        ColorPrint.print(0, [
            ("GRAY", f"State of assignment {ip}/{netmask}: "),
            ("YELLOW", "Waiting for IP assignment")
        ])
        currentTime = 0
        while True:
            currentTime += 1
            if currentTime > timeout:
                break
            (code, out) = self.container.exec_run("ip addr", stdout=True)
            asciiOut = out.decode("ascii")
            regexString = r"^[0-9]+: (?P<if_name>[a-zA-Z0-9]+(@[a-z-A-Z0-9]+)?:)(.*?)state (?P<state>([A-Z]+)) +\n(.* ?)\n( +)[a-z]+ (?P<ip>[0-9.?]+\/[0-9]+) brd (?P<broadcast>[0-9.?]+)"
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