api_database = {
    "name": "api_database",
    "environment": {
        
    },
    "database_ip": None,
    "database_port": 5432,
    "database_name": "api_server",
    "database_user": "postgres",
    "database_password": "password"
}

configuration_complete = False

class ApiDatabaseConfigurator:
    def get_config():
        if configuration_complete:
            return api_server
        raise Exception("Configuration not complete")
        