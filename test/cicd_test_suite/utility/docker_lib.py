import docker
import re
import time

class Container:
    def __init__(self, container):
        self.container = container
    def WaitRunningProcessOnPort(self, proto, local_address, state, timeout=30):
        currentTime = 0
        while True:
            currentTime += 1
            if currentTime > timeout:
                break
            (code, out) = self.container.exec_run("netstat -an", stdout=True)
            asciiOut = out.decode("ascii")
            regexString = r"^(?P<proto>((tcp)|(udp)))( )+[0-9]+( )+[0-9]+( )+(?P<local_address>([0-9.:]+))( )+(?P<foreign_address>[0-9.:*]+)( )+(?P<state>[A-Z]+)( )+$"
            regexIter = re.finditer(regexString, asciiOut, flags=re.MULTILINE)
            for i, match in enumerate(regexIter):
                print(f"Iteration number {i}")
                print(match.group("proto"))
                print(match.group("local_address"))
                print(match.group("foreign_address"))
                print(match.group("state"))
                if match.group("local_address") != local_address:
                    continue
                if match.group("state") != state:
                    continue
                if match.group("proto") != proto:
                    continue
                return
            time.sleep(1)
        raise Exception("Time for process to start timed out")
    def WaitIpAssignment(self, ip, netmask, timeout=30):
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
                print(match.group("if_name"))
                print(match.group("state"))
                print(match.group("ip"))
                print(match.group("broadcast"))
                if match.group("ip") != f"{ip}/{netmask}":
                    continue
                if match.group("state") != "UP":
                    continue
                return
            time.sleep(1)
        raise Exception("Time for ip assignment timed out")