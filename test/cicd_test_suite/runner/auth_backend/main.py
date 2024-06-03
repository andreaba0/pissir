from cicd_test_suite.backend.auth.routes.service.application import test_1 as test_a1 
from cicd_test_suite.backend.auth.routes.service.application import test_2 as test_a2 
from cicd_test_suite.backend.auth.routes.service.application import test_3 as test_a3 
from cicd_test_suite.backend.auth.routes.service.application import test_4 as test_a4

from cicd_test_suite.backend.auth.routes.profile import test_1 as test_p1

from cicd_test_suite.backend.auth.routes.company import test_1 as test_c1
from cicd_test_suite.backend.auth.routes.company import test_2 as test_c2

from cicd_test_suite.backend.auth.routes.apiaccess import test_1 as test_api1
from cicd_test_suite.backend.auth.routes.apiaccess import test_2 as test_api2


def EntryPoint(*args, **kwargs):
    test_a1.EntryPoint(*args, **kwargs)
    test_a2.EntryPoint(*args, **kwargs)
    test_a3.EntryPoint(*args, **kwargs)
    test_a4.EntryPoint(*args, **kwargs)
    test_p1.EntryPoint(*args, **kwargs)
    test_c1.EntryPoint(*args, **kwargs)
    test_c2.EntryPoint(*args, **kwargs)
    test_api1.EntryPoint(*args, **kwargs)
    test_api2.EntryPoint(*args, **kwargs)


def AuthMain():
    choice = 0
    while True:
        print("Component testing for Auth Backend")
        print("1. Setup container workflow") # won't do anything if containers are already running
        print("2. Rebuild and run database instance")
        print("3. Rebuild and run auth server instance")
        print("4. Rebuild and run fake OAuth provider instance")
        print("5. Run test routine")
        print("6. Tierdown workflow")
        print("6. Exit")
        choice = int(input("Enter your choice: "))
        if choice<1 or choice>5:
            print("Choice must be in range {1,5}")
            continue
        if choice == 1:
            runAuthDatabaseInstance()
            initAuthDatabase()
            runFakeOAuthProviderInstance()
            runAuthServerInstance()
            continue
        if choice == 2:
            setupBridgeAuthNetwork()
            continue
        if choice == 3:
            runAuthServerInstance()
            continue
        if choice == 4:
            TestRoutine()
            continue
        if choice == 5:
            break