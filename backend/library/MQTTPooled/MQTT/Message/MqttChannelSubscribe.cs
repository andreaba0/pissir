namespace MQTTConcurrent.Message;

public sealed class MqttChannelSubscribe : IMqttBusPacket {
    public string Topic { get; set; }
    public MqttChannelMessageType Type { get; }

    public MqttChannelSubscribe(string topic) {
        this.Topic = topic;
        this.Type = MqttChannelMessageType.SUBSCRIBE;
    }
}