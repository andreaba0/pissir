def green():
    return "\u001B[32m"

def red():
    return "\u001B[31m"

def gray():
    return "\u001B[90m"

def normal():
    return "\u001B[0m"

def white():
    return "\u001B[37m"

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
    def Contains(motd, expectedValues, actual):
        if actual in expectedValues:
            print(f"{gray()}{motd}: {green()}PASS{normal()}")
        else:
            print(f"{gray()}{motd}: {normal()}{red()}FAIL{normal()}")
            print(f"{tab()}{gray()}Expected: {expectedValues}")
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
    def __init__(self, motd):
        Group._motd = motd
        self._list = []
        self._tierup = None
        self._tierdown = None
        self._motd = motd
    def Add(self, method):
        self._list.append(method)
    def Run(self):
        print("\n")
        print(f"{white()}{Group._motd}{normal()}")
        self.__ExecuteTierUp()
        for method in self._list:
            try:
                method()
            except Exception as e:
                print(f"{red()} An exception occurred in a test method {normal()}")
                print(f"{red()} Test suite cancelled {normal()}")
                print(f"{' '*6} {red()} Error message: {normal()} {e}")
        self.__ExecuteTierDown()
        print("\n")
    
    def TierUp(self, method):
        self._tierup = method
        
    def TierDown(self, method):
        self._tierdown = method

    def __ExecuteTierUp(self):
        if self._tierup == None:
            return
        try:
            self._tierup()
        except Exception as e:
            print(f"{red()} An exception occurred in the tierup method {normal()}")
            print(f"{red()} Test suite cancelled {normal()}")
            print(f"{' '*6} {red()} Error message: {normal()} {e}")
    def __ExecuteTierDown(self):
        if self._tierdown == None:
            return
        try:
            self._tierdown()
        except Exception as e:
            print(f"{red()} An exception occurred in the tierdown method {normal()}")
            print(f"{red()} Data for next test suite may be inconsistent {normal()}")
            print(f"{' '*6} {red()} Error message: {normal()} {e.message}")
