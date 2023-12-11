using System.Threading.Tasks;
using System.Threading.Channels;

namespace MQTTConcurrent;

public interface IMQTTnetConcurrent
{
    public Task RunAsync();
    public Task PublishQueue(string topic, string message);
    public Task SubscribeQueue(string topic, Channel<MqttChannelMessage> channel);
    public Task UnsubscribeQueue(string topic);
}