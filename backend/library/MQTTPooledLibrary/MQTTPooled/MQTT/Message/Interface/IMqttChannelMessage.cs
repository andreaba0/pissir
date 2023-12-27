namespace MQTTConcurrent.Message;

public interface IMqttChannelMessage : IMqttBusPacket {
    public MqttChannelMessageType Type { get; }
    public string Topic { get; set; }
    public string Payload { get; set; }
}