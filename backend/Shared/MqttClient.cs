using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Formatter;
using MQTTnet.Protocol;

public class MqttClientPool
{
    private IMqttClient mqttClient;
    private MqttFactory mqttFactory;

    public async Task connectToBroker(
        string host,
        string port
    )
    {
        var mqttFactory = new MqttFactory();
        this.mqttFactory = mqttFactory;

        using (var mqttClient = mqttFactory.CreateMqttClient())
        {
            var mqttClientOptions = new MqttClientOptionsBuilder().WithTcpServer("127.0.0.1").WithProtocolVersion(MqttProtocolVersion.V500).Build();

            mqttClient.ApplicationMessageReceivedAsync += e =>
            {
                Console.WriteLine("### RECEIVED APPLICATION MESSAGE ###");
                Console.WriteLine(e.ApplicationMessage.Topic);

                return Task.CompletedTask;
            };
            // In MQTTv5 the response contains much more information.
            await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

            this.mqttClient = mqttClient;

            var mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
                .WithTopicFilter(
                    f =>
                    {
                        f.WithTopic("audit/test");
                    })
                .Build();

            await mqttClient.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);

            Console.WriteLine("### SUBSCRIBED ###");
            while(true) {}
        }
    }

    public async void subscribeToTopic(
        string topic
    )
    {
        //log typeof mqttFactory
        Console.WriteLine(mqttFactory.GetType());

    }
}