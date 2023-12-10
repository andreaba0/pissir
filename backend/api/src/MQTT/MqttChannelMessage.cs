namespace MQTTConcurrent;

public class MqttChannelMessage {
    public string Topic { get; set; }
    public string Message { get; set; }
    public MqttChannelMessageType Type { get; set; }

    public MqttChannelMessage(string topic, string message, MqttChannelMessageType type) {
        this.Topic = topic;
        this.Message = message;
        this.Type = type;
    }
}