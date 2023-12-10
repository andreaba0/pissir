using System.Threading.Channels;

namespace MQTTConcurrent;

public class RoundRobinDispatcher {
    private readonly Channel<MqttChannelMessage>[] channels;
    private int index;
    private object _lock = new object();

    public RoundRobinDispatcher(int poolSize) {
        this.channels = new Channel<MqttChannelMessage>[poolSize];
        for (int i = 0; i < poolSize; i++) {
            this.channels[i] = Channel.CreateUnbounded<MqttChannelMessage>();
        }
        this.index = 0;
    }

    public void Push(MqttChannelMessage message) {
        int currentIndex;
        lock (_lock) {
            currentIndex = this.index;
            this.index = (this.index + 1) % this.channels.Length;
        }
        this.channels[currentIndex].Writer.TryWrite(message);
    }

    public Channel<MqttChannelMessage> GetChannel(int index) {
        return this.channels[index];
    }

    public void PushAll(MqttChannelMessage message) {
        foreach (Channel<MqttChannelMessage> channel in this.channels) {
            channel.Writer.TryWrite(message);
        }
    }
}