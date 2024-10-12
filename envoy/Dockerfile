from envoyproxy/envoy:dev-bbc339afe2d65b40d3bacd55d242653d02df1bb5
run apt update && apt install -y net-tools iproute2

label maintainer="Andrea Barchietto"

copy ./config.yaml /etc/envoy/envoy.yaml

expose 10000

RUN chmod go+r /etc/envoy/envoy.yaml

CMD ["-c", "/etc/envoy/envoy.yaml", "--log-level", "debug"]