# Envoy

## Explanation of State.RESTART in live_demo test suite
```python
# Restarting proxy server is necessary and not doing so will make envoy essentially useless
# For more information see: /docs/Envoy.md
containers = StateManager.converge([
    Block(auth_server, authServerConfig, State.RUNNING),
    Block(auth_database, authDatabaseConfig, State.RUNNING),
    Block(oauth_server, oauthServerConfig, State.RUNNING),
    Block(api_server, apiServerConfig, State.RUNNING),
    Block(api_database, apiDatabaseConfig, State.RUNNING),
    Block(mosquitto_server, mosquittoServerConfig, State.RUNNING),
    Block(proxy_server, proxy_server_config, State.RESTART),
    Block(frontend_server, frontendServerConfig, State.RUNNING)
])
```
This snippet of code is from the file at: test/cicd_test_suite/live_demo/main.py
Before diving into why it is necessary to restart the proxy container, it is important to discuss what
happens inside a container when it is started.
When running a container with bridge network (the default network when starting a container using the test suite)
the newly created container will be assigned as dns server the ip address of the gateway of the bridge network or the dns server of the host machine.
Since Envoy proxy need to be able to resolve dns names of containers (see config.yaml in envoy/), it is necessary to allow envoy to access the dns server of docker.
The docker dns server is the only one that knows how to resolve the dns names of the containers that correspond to the names of the containers.
When connecting a container to a custom docker network, the file /etc/resolv.conf inside the container will be updated with the ip address of the dns server of Docker.
Since this update is done after the proxy is started, Envoy will continue to use the dns server of the host machine, a dns server that does not know how to resolve the dns names of the containers.
By restarting the container, envoy will finally be able to see the correct ip address of the dns server, and will be able to resolve dns names of the containers.
That is why it is necessary to restart the proxy container.