import sys

from component.backend.auth.main import AuthMain
from component.backend.api.main import ApiMain
from live_demo.main import IntegrationMain
from custom_env.main import custom_env_routine
from fake.token import fake_token_main as fake_token_main


def main():
    while True:
        print("1. Run test suite for authentication/authorization Microservice")
        print("2. Run test suite for API Microservice")
        print("3. Run integration test suite for both Microservices")
        print("4. Launch live sample environment")
        print("5. Run custom custom component")
        print("6. Generate fake token")
        print("7. Exit")
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
        if choice == "7":
            break
        if choice == "5":
            custom_env_routine.start()
            continue
        if choice == "6":
            role = input("Enter role (FA or WA): ")
            fake_token_main(role)
            continue

if __name__ == "__main__":
    main()