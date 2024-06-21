from dotenv import load_dotenv
import os

class env_manager:
    def get(key):
        load_dotenv()
        value = os.getenv(key)
        if value is None:
            raise Exception(key + " environment variable is not set")
        return value
    