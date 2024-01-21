namespace MQTTConcurrent.Message;

public sealed class MqttChannelMessage : IMqttChannelMessage {
    public string Topic { get; set; }
    public string Payload { get; set; }
    public MqttChannelMessageType Type { get; }
    public DateTime CreatedAt { get; } = DateTime.Now;
    public int Lifetime { get; } = 0;

    public MqttChannelMessage(string topic, string payload) {
        this.Topic = topic;
        this.Payload = payload;
        this.Type = MqttChannelMessageType.MESSAGE;
    }

    public MqttChannelMessage(string topic, string payload, int lifetime) {
        this.Topic = topic;
        this.Payload = payload;
        this.Type = MqttChannelMessageType.MESSAGE;
        this.Lifetime = lifetime;
    }
}