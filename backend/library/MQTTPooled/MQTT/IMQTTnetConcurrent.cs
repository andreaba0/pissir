using System.Threading.Tasks;
using System.Threading.Channels;

namespace MQTTConcurrent;

public interface IMQTTnetConcurrent
{
    public Task RunAsync();
    public void PublishQueue(string topic, string message);
    public void SubscribeQueue(string topic, Channel<MqttChannelMessage> channel);
    public void UnsubscribeQueue(string topic);
}