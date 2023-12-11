namespace MQTTConcurrent;

public interface IMqttChannelBus {
    public MqttChannelMessageType Type { get; }
}