namespace MQTTConcurrent.Message;

public sealed class MqttChannelUnsubscribe : IMqttBusPacket {
    public string Topic { get; set; }
    public MqttChannelMessageType Type { get; }

    public MqttChannelUnsubscribe(string topic) {
        this.Topic = topic;
        this.Type = MqttChannelMessageType.UNSUBSCRIBE;
    }
}