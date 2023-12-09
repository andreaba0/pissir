using MQTTnet;
using MQTTnet.Client;

namespace MQTTConcurrent;

public class MqttClientConcurrent : IMQTTnetConcurrent
{
    private readonly IManagedMqttClient mqttClient;
    private readonly ConnectionData cData;
    private Channel<string> topicSubscriptionChannel;
    private Channel<string> subscribedMessagesChannel;
    private Channel<string> publishChannel;
    public MqttClientConcurrent(ref readonly ConnectionData cData)
    {
        this.mqttClient = new MqttFactory().CreateManagedMqttClient();
        this.cData = cData;
    }

#if TEST
    public MqttClientConcurrent( ref readonly ConnectionData cData, IManagedMqttClient mqttClient ) {
        this.mqttClient = mqttClient;
        this.cData = cData;
    }
#endif

    public async Task<int> RunClient(CancellationToken ct)
    {
        await this.mqttClient.StartAsync(new ManagedMqttClientOptionsBuilder()
            .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
            .WithClientOptions(new MqttClientOptionsBuilder()
                .WithTcpServer(cData.host, cData.port)
                .WithCredentials(cData.username, cData.password)
                .WithProtocolVersion(MqttProtocolVersion.V500)
                .WithCleanSession()
                .Build())
            .Build());

        this.mqttClient.ApplicationMessageReceivedAsync += OnMessageReceivedAsync;
        while (!ct.IsCancellationRequested)
        {
            while(this.topicSubscriptionChannel.Reader.TryRead(out string topic))
            {
                ProcessTopicSubscription();
                ProcessPublish();
                ProcessSubscribedMessages();
            }
        }

        return 0;
    }

    internal Task ProcessTopicSubscription() {
        return Task.CompletedTask;
    }

    internal Task ProcessSubscribedMessages() {
        return Task.CompletedTask;
    }

    internal Task ProcessPublish() {
        return Task.CompletedTask;
    }

    internal Task OnMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs e)
    {
        return Task.CompletedTask;
    }
}