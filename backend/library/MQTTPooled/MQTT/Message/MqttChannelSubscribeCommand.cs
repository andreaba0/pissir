namespace MQTTConcurrent.Message;

public sealed class MqttChannelSubscribeCommand : IMqttBusPacket {
    public readonly string Topic { get; }
    public readonly MqttChannelMessageType Type { get; }
    public readonly Channel<MqttChannelMessage> Channel { get; }

    public MqttChannelSubscribeCommand(string topic, Channel<MqttChannelMessage> channel) {
        this.Topic = topic;
        this.Type = MqttChannelMessageType.SUBSCRIBE;
        this.Channel = channel;
    }
}