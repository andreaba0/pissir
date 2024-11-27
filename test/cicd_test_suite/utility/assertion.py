import sys
from io import StringIO

from utility.color_print import ColorPrint

class Assertion:
    def Equals(scope, motd, expected, actual):
        scope.add_assertion(motd, expected == actual, expected, actual)

class TestScope:
    def __init__(self):
        self.header = None
        self.assertions = []
        self.errored = False
        self.errored_message = None
        self.console_output = None
    
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
        self.middletier = None
        self.errored = False
        self.errored_message = None
    
    def set_tierup(self, fn):
        self.tierup = fn
    
    def set_tierdown(self, fn):
        self.tierdown = fn
    
    def set_middletier(self, fn):
        self.middletier = fn
    
    def add_assertion(self, assertion):
        self.assertions.append(assertion)
    
    def run(self):
        ColorPrint.print(
            0,
            [
                ('HEADER', "Running test suite")
            ]
        )

        # Intercepting stdout to display it later
        old_stdout = sys.stdout
        sys.stdout = StringIO()


        if self.tierup:
            try:
                self.tierup()
            except Exception as e:
                self.errored = True
                self.errored_message = f"Tierup failed: {e}, skipping test(s)"
                sys.stdout = old_stdout
                return
        
        for assertion in self.assertions:
            self.scopes.append(TestScope())
            self.scopes[-1].console_output = sys.stdout
            if self.middletier:
                try:
                    self.middletier()
                except Exception as e:
                    self.scopes[-1].errored = True
                    self.scopes[-1].errored_message = f"Middletier failed: {e}, skipping test(s)"
                    sys.stdout = old_stdout
                    return
            try:
                assertion(self.scopes[-1])
            except Exception as e:
                self.scopes[-1].errored = True
                self.scopes[-1].errored_message = f"Assertion failed: {e}, maybe it's a python code error in a test function"
                continue

        if self.tierdown:
            try:
                self.tierdown()
            except Exception as e:
                self.errored = True
                self.errored_message = f"Tierdown failed: {e}, skipping test(s)"
                sys.stdout = old_stdout
                return
        
        sys.stdout = old_stdout
    
    def print_stats(self):
        ColorPrint.print(
            0,
            [
                ('HEADER', "Displaying test suite statistics")
            ]
        )
        if self.errored:
            ColorPrint.print(
                4,
                [
                    ('FAIL', f"Errored: {self.errored_message}, skipping test cases")
                ]
            )
            return

        has_errors = False
        for scope in self.scopes:
            ColorPrint.print(
                4,
                [
                    ('GRAY', f"Stats for: {scope.header}")
                ]
            )
            passed = 0
            failed = 0
            if scope.errored:
                has_errors = True
                ColorPrint.print(
                    4,
                    [
                        ('FAIL', f"Errored: {scope.errored_message}")
                    ]
                )
                continue
            for assertion in scope.assertions:
                motd = assertion['motd'][:50] + '...' if len(assertion['motd']) > 50 else assertion['motd'].ljust(53)
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
                    ('GRAY', f"Generic statistics:")
                ]
            )
            if not has_errors:
                ColorPrint.print(
                    8,
                    [

                        ('OKGREEN', f"Passed: {passed}"),
                        ('GRAY', " | "),
                        ('FAIL', f"Failed: {failed}"),
                        ('GRAY', " | "),
                        ('HEADER', f"Total: {passed + failed}"),
                        ('GRAY', " | "),
                        ('HEADER', f"Pass rate: {round(passed / (passed + failed) * 100, 2)}%")
                    ]
                )
            if has_errors:
                ColorPrint.print(
                    8,
                    [
                        ('FAIL', f"Errors at python level code may have hidden some test results, altering the final statistics")
                    ]
                )

            ColorPrint.print(
                0,
                [
                    ('HEADER', "Test output for"),
                    ('GRAY', f" {scope.header}")
                ]
            )
            console_output = scope.console_output.getvalue()
            ColorPrint.print(
                0,
                [
                    ('GRAY', f"{console_output}")
                ]
            )

