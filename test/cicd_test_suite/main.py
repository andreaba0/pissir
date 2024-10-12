from component.backend.auth.main import AuthMain
from component.backend.api.main import ApiMain
from live_demo.main import IntegrationMain
from custom_env.main import custom_env_routine


def main():
    while True:
        print("1. Run test suite for authentication/authorization Microservice")
        print("2. Run test suite for API Microservice")
        print("3. Run integration test suite for both Microservices")
        print("4. Launch live sample environment")
        print("5. Run custom custom component")
        print("6. Exit")
        choice = input("Enter your choice: ")
        if choice == "1":
            AuthMain()
            continue
        if choice == "2":
            ApiMain()
            continue
        if choice == "4":
            IntegrationMain()
            continue
        if choice == "6":
            break
        if choice == "5":
            custom_env_routine.start()
            continue

if __name__ == "__main__":
    main()