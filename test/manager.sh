#!/bin/bash

# check that there is exactly 1 argument
if [ "$#" -ne 1 ]; then
    echo "Usage: $0 Should be called with exactly one argument. The name of the action to perform. Either 'build' or 'start'"
    exit 1
fi

#assign argument to variable obj
obj=$1

# check that argument is auth
if [ $obj = "build" ]; then
    
    ctime=$(date +%s)
    echo "Building test_suite:$ctime"


    docker build -t test_suite:$ctime -t test_suite:latest -f Dockerfile .
    exit 0
fi

# check that argument is api
if [ $obj = "start" ]; then

    ctime=$(date +%s)

    echo "Starting test_suite:$ctime"

    echo "When container starts, run this command to start the test suite:"
    echo "poetry run python3 cicd_test_suite/main.py"


    docker run \
    -e IP_ADDRESS_SPACE='172.16.10.0/26' \
    -v /var/run/docker.sock:/var/run/docker.sock \
    --label test_suite \
    --network host \
    -i \
    -t \
    test_suite:latest \
    /bin/bash


    exit 0
fi