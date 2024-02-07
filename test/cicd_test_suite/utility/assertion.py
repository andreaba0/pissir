from utility.color_print import ColorPrint

class Assertion:
    def Equals(scope, motd, expected, actual):
        scope.add_assertion(motd, expected == actual, expected, actual)

class TestScope:
    def __init__(self):
        self.header = None
        self.assertions = []
    
    def set_header(self, header):
        self.header = header
    
    def add_assertion(self, motd, has_passed, expected, actual):
        self.assertions.append({
            'motd': motd,
            'has_passed': has_passed,
            'expected': expected,
            'actual': actual
        })

class TestSuite:
    def __init__(self):
        self.scopes = []
        self.assertions = []
        self.tierup = None
        self.tierdown = None
    
    def set_tierup(self, fn):
        self.tierup = fn
    
    def set_tierdown(self, fn):
        self.tierdown = fn
    
    def add_assertion(self, assertion):
        self.assertions.append(assertion)
    
    def run(self):
        if self.tierup:
            try:
                self.tierup()
            except Exception as e:
                ColorPrint.print(
                    4,
                    [
                        ('FAIL', f"Tierup failed: {e}, skipping test")
                    ]
                )
                return
            
        for assertion in self.assertions:
            try:
                self.scopes.append(TestScope())
                assertion(self.scopes[-1])
            except Exception as e:
                ColorPrint.print(
                    4,
                    [
                        ('FAIL', f"Assertion failed: {e}")
                    ]
                )
                return
        
        if self.tierdown:
            try:
                self.tierdown()
            except Exception as e:
                ColorPrint.print(
                    4,
                    [
                        ('FAIL', f"Tierdown failed: {e}")
                    ]
                )
                return
    
    def print_stats(self):
        for scope in self.scopes:
            ColorPrint.print(
                4,
                [
                    ('GRAY', f"Stats for: {scope.header}")
                ]
            )
            passed = 0
            failed = 0
            for assertion in scope.assertions:
                #new motd string of max length 20
                motd = assertion['motd'][:20] + '...' if len(assertion['motd']) > 20 else assertion['motd']
                text = 'PASS' if assertion['has_passed'] else 'FAIL'
                color = 'OKGREEN' if assertion['has_passed'] else 'FAIL'
                ColorPrint.print(
                    8,
                    [
                        ('GRAY', f"Test: <{motd}>"),
                        (color, f" {text} ")
                    ]
                )
                if assertion['has_passed']:
                    passed += 1  
                else:
                    failed += 1
                    ColorPrint.print(
                        12,
                        [
                            ('GRAY', f"Expected:   "),
                            ('OKBLUE', f"{assertion['expected']}")
                        ],
                        [
                            ('GRAY', f"Actual:     "),
                            ('OKBLUE', f"{assertion['actual']}")
                        ]
                    )
            ColorPrint.print(
                8,
                [
                    ('GRAY', f"General statistics:")
                ],
                [

                    ('OKGREEN', f"Passed: {passed}")
                ],
                [
                    ('FAIL', f"Failed: {failed}")
                ]
            )