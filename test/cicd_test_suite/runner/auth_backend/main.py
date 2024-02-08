from cicd_test_suite.backend.auth.routes.service.application import test_1, test_2


def EntryPoint(*args, **kwargs):
    test_1.EntryPoint(*args, **kwargs)
    test_2.EntryPoint(*args, **kwargs)