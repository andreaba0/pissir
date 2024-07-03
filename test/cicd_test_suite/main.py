import custom_env.main as custom_env

from cicd_test_suite.component.backend.auth.main import AuthMain


def main():
    while True:
        print("1. Run auth testing environment")
        print("2. Run api testing environment")
        print("3. Run integration testing environment")
        print("4. Go live")
        print("5. Run custom environment")
        print("6. Exit")
        choice = input("Enter your choice: ")
        if choice == "1":
            AuthMain()
            continue
        if choice == "6":
            break
        if choice == "5":
            custom_env.custom_env_routine.start()
            continue

if __name__ == "__main__":
    main()