namespace MQTTConcurrent.Message;

public interface IMqttChannelMessage {
    public MqttChannelMessageType Type { get; }
    public string Topic { get; set; }
    public string Message { get; set; }
}