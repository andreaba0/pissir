using System.Threading.Channels;

namespace MQTTConcurrent.Message;

public sealed class MqttChannelUnsubscribeCommand : IMqttBusPacket {
    public string Topic { get; }
    public MqttChannelMessageType Type { get; }
    public Channel<IMqttChannelMessage> Channel { get; }

    public MqttChannelUnsubscribeCommand(string topic, Channel<IMqttChannelMessage> channel) {
        this.Topic = topic;
        this.Type = MqttChannelMessageType.UNSUBSCRIBE;
        this.Channel = channel;
    }
}