using System.Threading.Channels;

namespace MQTTConcurrent.Message;

public sealed class MqttChannelSubscribeCommand : IMqttBusPacket {
    public string Topic {get;}
    public MqttChannelMessageType Type {get;}
    public Channel<IMqttChannelMessage> Channel {get;}

    public MqttChannelSubscribeCommand(string topic, Channel<IMqttChannelMessage> channel) {
        this.Topic = topic;
        this.Type = MqttChannelMessageType.SUBSCRIBE;
        this.Channel = channel;
    }
}