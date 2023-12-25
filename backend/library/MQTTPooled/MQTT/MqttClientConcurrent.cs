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

namespace MQTTConcurrent;

public class MqttClientConcurrent
{
    private readonly IManagedMqttClient mqttClient;
    private readonly ConnectionData cData;
    private readonly Channel<IMqttBusPacket> sendChannel;
    private readonly Channel<IMqttBusPacket> receiveChannel;
    public MqttClientConcurrent(
        ConnectionData cData,
        Channel<IMqttBusPacket> sendChannel,
        Channel<IMqttBusPacket> receiveChannel
    )
    {
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

    public async Task<int> RunClient(CancellationToken ct) {
        await this.mqttClient.StartAsync(new ManagedMqttClientOptionsBuilder()
            .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
            .WithClientOptions(new MqttClientOptionsBuilder()
                .WithTcpServer(cData.host, cData.port)
                //.WithCredentials(cData.username, cData.password)
                .WithProtocolVersion(MqttProtocolVersion.V500)
                .WithCleanSession()
                .Build())
            .Build());

        this.mqttClient.ApplicationMessageReceivedAsync += OnMessageReceivedAsync;
        while (!ct.IsCancellationRequested)
        {
            IMqttChannelBus message = await this.sendChannel.Reader.ReadAsync(ct);
            if (message is Message.MqttChannelSubscribe subscribeMessage)
            {
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
        return 0;
    }

    internal Task ProcessTopicSubscription(Message.MqttChannelSubscribe message)
    {
        List<MqttTopicFilter> topicFilters = new List<MqttTopicFilter>();
        topicFilters.Add(new MqttTopicFilterBuilder()
            .WithTopic(message.Topic)
            .Build());
        this.mqttClient.SubscribeAsync(topicFilters);
        return Task.CompletedTask;
    }

    internal Task ProcessTopicUnsubscribe(string topic)
    {
        this.mqttClient.UnsubscribeAsync(new string[] { topic });
        return Task.CompletedTask;
    }

    internal Task ProcessPublish(IMqttChannelMessage message)
    {
        this.mqttClient.PublishAsync(new MqttApplicationMessageBuilder()
            .WithTopic(message.Topic)
            .WithPayload(message.Message)
            .WithExactlyOnceQoS()
            .Build());
        return Task.CompletedTask;
    }

    internal Task OnMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs e)
    {
        IMqttChannelBus message = new Message.MqttChannelMessage(
            e.ApplicationMessage.Topic, 
            e.ApplicationMessage.ConvertPayloadToString()
        );
        this.receiveChannel.Writer.WriteAsync(message);
        return Task.CompletedTask;
    }
}