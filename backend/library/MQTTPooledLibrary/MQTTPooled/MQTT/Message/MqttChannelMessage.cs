namespace MQTTConcurrent.Message;

public sealed class MqttChannelMessage : IMqttChannelMessage {
    public string Topic { get; set; }
    public string Payload { get; set; }
    public MqttChannelMessageType Type { get; }

    public MqttChannelMessage(string topic, string payload) {
        this.Topic = topic;
        this.Payload = payload;
        this.Type = MqttChannelMessageType.MESSAGE;
    }
}