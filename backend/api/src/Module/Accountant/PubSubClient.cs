using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Channels;

using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;

namespace Module.Accountant;

public class PubSubClient: IPubSubClient {
    private readonly IMqttClient _mqttClient;
    private bool wasConnected = false;
    public PubSubClient(
        IMqttClient mqttClient
    ) {
        _mqttClient = mqttClient;
        _mqttClient.ApplicationMessageReceivedAsync+= GotMessage;
    }

    public async Task<int> Routine( CancellationToken ct) {
        while(!ct.IsCancellationRequested) {
            if(await ConnectOnce()) {
                //thread sleep 100ms
                Thread.Sleep(100);
            }
        }
        return 0;
    }

    internal async Task<bool> ConnectOnce() {
        if(_mqttClient.IsConnected) {
            wasConnected = true;
            return true;
        }
        try {
            if(wasConnected) {
                wasConnected = false;
                Console.WriteLine("Connection lost, reconnecting...");
            }
            await _mqttClient.ConnectAsync(GenerateOptions());
            Console.WriteLine("Connected to broker");

            var mqttChannelOptions = new MqttClientSubscribeOptionsBuilder()
                .WithTopicFilter("test")
                .Build();
            
            await _mqttClient.SubscribeAsync(mqttChannelOptions);

            return true;
        } catch(Exception e) {
            return false;
        }
        return false;
    }

    private async Task GotMessage(MqttApplicationMessageReceivedEventArgs e) {
        Console.WriteLine("Got message");
        Console.WriteLine(e.ApplicationMessage.Topic);
        Console.WriteLine(e.ApplicationMessage.ConvertPayloadToString());
    }

    public static MqttClientOptions GenerateOptions() {
        var options = new MqttClientOptionsBuilder()
            .WithClientId("Accountant")
            .WithTcpServer("localhost", 1883)
            .WithCredentials("test", "test")
            .WithCleanSession()
            .Build();
        return options;
    }
}