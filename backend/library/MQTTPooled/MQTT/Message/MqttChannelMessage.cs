namespace MQTTConcurrent.Message;

public sealed class MqttChannelMessage : IMqttBusPacket {
    public string Topic { get; set; }
    public string Message { get; set; }
    public MqttChannelMessageType Type { get; }

    public MqttChannelMessage(string topic, string message) {
        this.Topic = topic;
        this.Message = message;
        this.Type = MqttChannelMessageType.MESSAGE;
    }
}