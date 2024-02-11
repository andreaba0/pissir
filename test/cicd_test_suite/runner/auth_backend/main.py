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