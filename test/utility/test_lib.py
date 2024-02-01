def green():
    return "\u001B[32m"

def red():
    return "\u001B[31m"

def gray():
    return "\u001B[90m"

def normal():
    return "\u001B[0m"

def tab():
    return " " * 4

def AssertGroup(motd, method):
    print("\n")
    print(f"\u001B[90m{motd}\u001B[0m")
    method()
    print("\n")

class Assert:
    def Equals(motd, expected, actual):
        if expected == actual:
            print(f"{gray()}{motd}: {green()}PASS{normal()}")
        else:
            print(f"{gray()}{motd}: {normal()}{red()}FAIL{normal()}")
            print(f"{tab()}{gray()}Expected: {expected}")
            print(f"{tab()}{gray()}Actual: {actual}")
    def IsTrue(motd, actual):
        if actual:
            print(f"\u001B[32mPASS\u001B[0m")
        else:
            print(f"\u001B[31mFAIL\u001B[0m")
            print(f"Expected: True")
            print(f"Actual: {actual}")
    def IsFalse(motd, actual):
        if not actual:
            print(f"\u001B[32mPASS\u001B[0m")
        else:
            print(f"\u001B[31mFAIL\u001B[0m")
            print(f"Expected: False")
            print(f"Actual: {actual}")

class Group:
    _list = []
    _motd = ""
    def __init__(self, motd):
        Group._motd = motd
    def Add(self, method):
        Group._list.append(method)
    def Run(self):
        print("\n")
        print(f"{gray()}{Group._motd}{normal()}")
        for method in Group._list:
            method()
        print("\n")