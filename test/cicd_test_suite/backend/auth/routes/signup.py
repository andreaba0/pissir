from utility import TestSuite, Assertion

def test1(scope):
    scope.set_header('Test 1')
    Assertion.Equals(scope, 'Test 1', 1, 1)
    Assertion.Equals(scope, 'Test 2', 1, 2)

def test2(scope):
    scope.set_header('Signup with error')
    Assertion.Equals(scope, 'Should reject user signup if bearer token is not provided', 1, 1)
    Assertion.Equals(scope, 'Test 2', 1, 1)
    Assertion.Equals(scope, 'Test 3', 1, 1)
    Assertion.Equals(scope, 'Test 4', 1, 2)
    

def EntryPoint():
    suite = TestSuite()
    suite.add_assertion(test1)
    suite.add_assertion(test2)
    suite.run()
    suite.print_stats()