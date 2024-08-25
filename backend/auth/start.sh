#/bin/bash

# Start the auth service
DOTNET_ENV_DATABASE_HOST=172.17.0.5 \
DOTNET_ENV_DATABASE_PORT=5432 \
DOTNET_ENV_DATABASE_USER=postgres \
DOTNET_ENV_DATABASE_PASSWORD=password \
DOTNET_ENV_DATABASE_NAME=auth_db \
DOTNET_ENV_PISSIR_ISS=https://appweb.andreabarchietto.it \
DOTNET_ENV_PISSIR_AUD=https://pissir.andreabarchietto.it \
DOTNET_ENV_WEBSERVER_BOUND=0.0.0.0:8000 \
dotnet run