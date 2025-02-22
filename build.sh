#!/bin/bash

# check that there is exactly 1 argument
if [ "$#" -ne 1 ]; then
    echo "Usage: $0 Should be called with exactly one argument. The name of the object to build"
    exit 1
fi

#assign argument to variable obj
obj=$1

# check that argument is auth
if [ $obj = "auth" ]; then
    cd backend
    # create variable ctime with current seconds since epoch
    ctime=$(date +%s)
    
    echo "Building pissir_auth_server:$ctime"
    # docker build Dockerfile.backend.auth with tag pissir_auth_server:ctime
    docker build -t pissir_auth_server:$ctime -t pissir_auth_server:latest -f Dockerfile.backend.auth .
    exit 0
fi

# check that argument is api
if [ $obj = "api" ]; then
    cd backend
    # create variable ctime with current seconds since epoch
    ctime=$(date +%s)

    echo "Building pissir_api_server:$ctime"
    # docker build Dockerfile.backend.api with tag pissir_api_server:ctime
    docker build -t pissir_api_server:$ctime -t pissir_api_server:latest -f Dockerfile.backend.api .
    exit 0
fi

if [ $obj = "auth_database" ]; then
    cd database/auth
    ctime=$(date +%s)
    docker build -t pissir_auth_database:$ctime -t pissir_auth_database:latest -f Dockerfile .
    exit 0
fi

if [ $obj = "api_database" ]; then
    cd database/api
    ctime=$(date +%s)
    docker build -t pissir_api_database:$ctime -t pissir_api_database:latest -f Dockerfile .
    exit 0
fi

if [ $obj = "proxy" ]; then
    cd envoy
    ctime=$(date +%s)
    docker build -t appweb_proxy_server:$ctime -t appweb_proxy_server:latest -f Dockerfile .
    exit 0
fi

if [ $obj = "fake_oauth" ]; then
    cd fake_oauth_server
    ctime=$(date +%s)
    docker build -t appweb_fake_oauth_server:$ctime -t appweb_fake_oauth_server:latest -f Dockerfile .
    exit 0
fi

if [ $obj = "mosquitto" ]; then
    cd mosquitto
    ctime=$(date +%s)
    docker build -t pissir_broker_server:$ctime -t pissir_broker_server:latest -f Dockerfile .
    exit 0
fi

if [ $obj = "frontend" ]; then
    cd frontend
    ctime=$(date +%s)
    docker build -t appweb_frontend_server:$ctime -t appweb_frontend_server:latest -f Dockerfile .
    exit 0
fi
    