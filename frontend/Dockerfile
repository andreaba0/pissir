from mcr.microsoft.com/dotnet/aspnet:7.0 as dotnet8_runtime_nettools
run apt update && apt install -y net-tools iproute2

from mcr.microsoft.com/dotnet/sdk:8.0 as build


run mkdir -p /app/frontend

copy ./frontend.csproj /app/frontend
copy ./frontend.csproj.user /app/frontend

workdir /app/frontend
run dotnet restore --no-cache frontend.csproj


WORKDIR /app/frontend


# Copy source code
COPY ./Models /app/frontend/Models
COPY ./Pages /app/frontend/Pages
#COPY ./Properties /app/frontend/Properties
COPY ./Program.cs /app/frontend

workdir /app/frontend

env ASPNETCORE_ENVIRONMENT=Testing
env DOTNET_ENVIRONMENT=Testing

run dotnet build -o /publish_compiled --no-restore

from dotnet8_runtime_nettools as final_runtime
run mkdir -p /app_compiled/shared_log
copy --from=build /publish_compiled /app_compiled
#copy ./auth/src/appsettings.json /app_compiled
#copy /auth/src/appsettings.Testing.json /app_compiled
workdir /app_compiled
COPY ./Styles /app_compiled/Styles
COPY ./wwwroot /app_compiled/wwwroot

env ASPNETCORE_ENVIRONMENT=Testing
env DOTNET_ENVIRONMENT=Testing

expose 8000

#run dotnet and redirect output to file /app_compiled/log.txt
entrypoint ["dotnet", "frontend.dll"]
CMD ["--urls", "http://0.0.0.0:8000"]