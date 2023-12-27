namespace MQTTConcurrent;

public interface IMqttBusPacket {
    public MqttChannelMessageType Type { get; }
}