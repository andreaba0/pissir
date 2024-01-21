namespace MQTTConcurrent;

public interface IMQTTnetConcurrent
{
    public MqttClientConcurrent(string connectionString);
    public Task RunAsync();
    public void PublishQueue(string topic, string message);
    public void SubscribeQueue(string topic, Channel channel);
    public void UnsubscribeQueue(string topic);
}