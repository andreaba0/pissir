FROM python:3.10-slim as base
# Install Poetry
RUN pip3 install poetry==1.7.1
WORKDIR /cicd_test_suite
COPY pyproject.toml poetry.lock /cicd_test_suite
# Install dependencies
RUN poetry install


COPY cicd_test_suite /cicd_test_suite/cicd_test_suite
COPY README.md /cicd_test_suite


#ENTRYPOINT ["poetry", "run", "python3", "cicd_test_suite/main.py"]