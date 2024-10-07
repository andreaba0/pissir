using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Formatter;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Protocol;
using MQTTnet.Server;
using MQTTnet.Packets;
using System;
using System.Threading;
using System.Threading.Channels;

using MQTTConcurrent.Message;

namespace MQTTConcurrent;

/// <summary>
/// MqttClientConcurrent is a wrapper around MQTTnet's IManagedMqttClient.
/// It creates 
/// </summary>
public class MqttClientConcurrent
{
    private readonly IManagedMqttClient mqttClient;
    private readonly ConnectionData cData;
    private readonly Channel<IMqttBusPacket> sendChannel;
    private readonly Channel<IMqttBusPacket> receiveChannel;
    private bool isConnected = false;
    public MqttClientConcurrent(
        ConnectionData cData,
        Channel<IMqttBusPacket> sendChannel,
        Channel<IMqttBusPacket> receiveChannel
    )
    {
        /*
            ManagerClient will handle topic subscriptions across connections.
            So, no need to resubscribe to a topic when a new connection is made after a disconnect.
            More info: MQTTnet GitHub wiki/ManagedClient
        */
        this.mqttClient = new MqttFactory().CreateManagedMqttClient();
        
        
        this.cData = cData;
        this.sendChannel = sendChannel;
        this.receiveChannel = receiveChannel;
    }

#if TEST
    public MqttClientConcurrent(ConnectionData cData, IManagedMqttClient mqttClient ) {
        this.mqttClient = mqttClient;
        this.cData = cData;
    }
#endif

    private void ClientConnectionStatusInfo() {
        if (this.mqttClient.IsConnected&& !this.isConnected) {
            this.isConnected = true;
            Console.WriteLine("MqttClientConcurrent connected");
        } else if (!this.mqttClient.IsConnected && this.isConnected) {
            this.isConnected = false;
            Console.WriteLine("MqttClientConcurrent not connected");
        }
    }

    public async Task<int> RunClient(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        try
        {
            await this.mqttClient.StartAsync(new ManagedMqttClientOptionsBuilder()
                .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
                .WithClientOptions(new MqttClientOptionsBuilder()
                    .WithTcpServer(cData.host, cData.port)
                    //.WithCredentials(cData.username, cData.password)
                    .WithProtocolVersion(MqttProtocolVersion.V500)
                    .WithCleanSession()
                    .Build())
                .Build());
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        this.mqttClient.ConnectedAsync += async (e) => {
            ClientConnectionStatusInfo();
        };
        this.mqttClient.DisconnectedAsync += async (e) => {
            ClientConnectionStatusInfo();
        };
        this.mqttClient.ApplicationMessageReceivedAsync += OnMessageReceivedAsync;
        bool previousConnectionState = true;
        while (!ct.IsCancellationRequested)
        {
            IMqttBusPacket message = await this.sendChannel.Reader.ReadAsync(ct);
            if (message is Message.MqttChannelSubscribe subscribeMessage)
            {
                Console.WriteLine($"Client is subscribing to topic {subscribeMessage.Topic}");
                ProcessTopicSubscription(subscribeMessage);
                continue;
            }
            if (message is Message.MqttChannelUnsubscribe unsubscribeMessage)
            {
                ProcessTopicUnsubscribe(unsubscribeMessage.Topic);
                continue;
            }
            if (message is Message.MqttChannelMessage publishMessage)
            {
                ProcessPublish(publishMessage);
                continue;
            }
        }
        if (ct.IsCancellationRequested) {
            Console.WriteLine("MqttClientConcurrent stopped");
            throw new OperationCanceledException();
        }
        return 0;
    }

    internal Task ProcessTopicSubscription(Message.MqttChannelSubscribe message)
    {
        List<MqttTopicFilter> topicFilters = new List<MqttTopicFilter>();
        topicFilters.Add(new MqttTopicFilterBuilder()
            .WithTopic(message.Topic)
            .Build());
        this.mqttClient.SubscribeAsync(topicFilters).Wait();
        return Task.CompletedTask;
    }

    internal Task ProcessTopicUnsubscribe(string topic)
    {
        this.mqttClient.UnsubscribeAsync(new string[] { topic });
        return Task.CompletedTask;
    }

    internal Task ProcessPublish(IMqttChannelMessage message)
    {
        //eventually drop messages that have expired
        //this avoids possible internal DDoS attack when mqtt broker comes back online after a long period of time
        if (message.Lifetime != 0 && DateTime.Now > message.CreatedAt.AddSeconds(message.Lifetime))
        {
            return Task.CompletedTask;
        }

        this.mqttClient.EnqueueAsync(new MqttApplicationMessageBuilder()
            .WithTopic(message.Topic)
            .WithPayload(message.Payload)
            .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtMostOnce)
            .Build());
        return Task.CompletedTask;
    }

    internal Task OnMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs e)
    {
        Console.WriteLine($"Received message on topic {e.ApplicationMessage.Topic}");
        IMqttChannelMessage message = new Message.MqttChannelMessage(
            e.ApplicationMessage.Topic,
            e.ApplicationMessage.ConvertPayloadToString()
        );
        this.receiveChannel.Writer.WriteAsync(message);
        return Task.CompletedTask;
    }
}