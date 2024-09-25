using System.Threading.Channels;

namespace MQTTConcurrent.Message;

public sealed class MqttChannelUnsubscribeCommand : IMqttBusPacket {
    public string Topic { get; }
    public MqttChannelMessageType Type { get; }
    public Channel<IMqttChannelMessage> ResponseChannel { get; }

    public MqttChannelUnsubscribeCommand(string topic, Channel<IMqttChannelMessage> responseChannel) {
        this.Topic = topic;
        this.Type = MqttChannelMessageType.UNSUBSCRIBE;
        this.ResponseChannel = responseChannel;
    }
}