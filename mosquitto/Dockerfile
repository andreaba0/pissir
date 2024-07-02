from eclipse-mosquitto:2.0.18

COPY mosquitto.conf /mosquitto/config/mosquitto.conf
COPY mosquitto.acl /mosquitto/config/mosquitto.acl

RUN chown -R mosquitto:mosquitto /mosquitto/config
RUN chmod 0700 /mosquitto/config/mosquitto.acl

USER mosquitto

EXPOSE 1883

WORKDIR /mosquitto/config

CMD ["/usr/sbin/mosquitto", "-c", "/mosquitto/config/mosquitto.conf"]