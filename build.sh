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

    # docker build Dockerfile.backend.api with tag pissir_api_server:ctime
    docker build -t pissir_api_server:$ctime -f Dockerfile.backend.api .
    exit 0
fi

if [ $obj = "auth_database" ]; then
    cd database/auth
    ctime=$(date +%s)
    docker build -t pissir_auth_database:$ctime -f Dockerfile .
    exit 0
fi

if [ $obj = "envoy" ]; then
    cd envoy
    ctime=$(date +%s)
    docker build -t appweb_envoy:$ctime -f Dockerfile .
    exit 0
fi

if [ $obj = "fake_oauth" ]; then
    cd fake_oauth_server
    ctime=$(date +%s)
    docker build -t appweb_fake_oauth_server:$ctime -f Dockerfile .
    exit 0
fi
    