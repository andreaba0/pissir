namespace MQTTConcurrent.Message;

public sealed class MqttChannelUnsubscribeCommand : IMqttBusPacket {
    public readonly string Topic { get; }
    public readonly MqttChannelMessageType Type { get; }
    public readonly Channel<MqttChannelMessage> Channel { get; }

    public MqttChannelUnsubscribeCommand(string topic, Channel<MqttChannelMessage> channel) {
        this.Topic = topic;
        this.Type = MqttChannelMessageType.UNSUBSCRIBE;
        this.Channel = channel;
    }
}