using System.Threading.Channels;

namespace MQTTConcurrent;

public class RoundRobinDispatcher {
    private readonly Channel<IMqttChannelBus>[] channels;
    private int index;
    private object _lock = new object();

    public RoundRobinDispatcher(int poolSize) {
        this.channels = new Channel<IMqttChannelBus>[poolSize];
        for (int i = 0; i < poolSize; i++) {
            this.channels[i] = Channel.CreateUnbounded<IMqttChannelBus>();
        }
        this.index = 0;
    }

    public async Task Push(IMqttChannelBus message) {
        int currentIndex;
        lock (_lock) {
            currentIndex = this.index;
            this.index = (this.index + 1) % this.channels.Length;
        }
        await this.channels[currentIndex].Writer.WriteAsync(message);
    }

    public Channel<IMqttChannelBus> GetChannel(int index) {
        return this.channels[index];
    }

    public async Task PushAll(IMqttChannelBus message) {
        if(message.Type!=MqttChannelMessageType.SUBSCRIBE&&message.Type!=MqttChannelMessageType.UNSUBSCRIBE) {
            throw new MqttConcurrentException("Only subscribe and unsubscribe messages can be pushed to all channels");
        }
        foreach (Channel<IMqttChannelBus> channel in this.channels) {
            await channel.Writer.WriteAsync(message);
        }
    }
}