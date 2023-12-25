using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;

namespace MQTTConcurrent;

public class MQTTnetConcurrent : IMQTTnetConcurrent, IDisposable
{
    private readonly ConnectionData cData;
    private Dictionary<string, List<Channel<IMqttChannelMessage>>> subscriptionChannels
    private Channel<IMqttChannelMessage> demuxChannel;
    private RoundRobinDispatcher dispatcher;
    private Channel<IMqttBusPacket> sharedInputChannel;

    public MQTTnetConcurrent(
        string connectionString,
        Channel<IMqttBusPacket> sharedInputChannel
        )
    {
        this.cData = ConnectionData.Parse(connectionString);
        this.subscribeChannels = new Dictionary<string, List<Channel<IMqttChannelMessage>>>();
        this.dispatcher = new RoundRobinDispatcher(cData.poolSize);
        this.demuxChannel = Channel.CreateUnbounded<IMqttBusPacket>();
        this.sharedInputChannel = sharedInputChannel;
    }

#if TEST
    public MQTTnetConcurrent(
        ConnectionData cData, 
        RoundRobinDispatcher dispatcher,
        Channel<IMqttBusPacket> demuxChannel,
        Channel<MqttChannelMessage> sharedInputChannel
        Dictionary<string, List<Channel<IMqttChannelMessage>>> subscribeChannels
    ) {
        this.cData = cData;
        this.dispatcher = dispatcher;
        this.subscribeChannels = subscribeChannels;
        this.demuxChannel = demuxChannel;
        this.sharedInputChannel = sharedInputChannel;
    }
#endif

    public Task RunAsync(CancellationToken ct)
    {
        MqttClientConcurrent[] mqttClients = new MqttClientConcurrent[cData.poolSize];
        Task[] workers = new Task[cData.poolSize + 2];
        for (int i = 0; i < cData.poolSize; i++)
        {
            Channel<IMqttBusPacket> clientChannel = this.dispatcher.GetChannel(i);
            mqttClients[i] = new MqttClientConcurrent(
                cData,
                clientChannel,
                this.demuxChannel
            );
            workers[i] = Task.Factory.StartNew(
                async () => await mqttClients[i].RunClient(ct), TaskCreationOptions.LongRunning
            ).Unwrap();
        }
        workers[cData.poolSize] = Task.Run(async () => await MessageDispatcherRoutine(ct));
        workers[cData.poolSize + 1] = Task.Run(async () => await MessageDemuxRoutine(ct));
        Task.WaitAll(workers);
        return Task.CompletedTask;
    }

    /**
     * dispatch messages from a shared unbounded channel to mqtt client pool
     */
    internal async Task MessageDispatcherRoutine(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            IMqttBusPacket message = await this.sharedInputChannel.Reader.ReadAsync(ct);
            if (message is MqttChannelMessage mqttMessage)
            {
                this.dispatcher.Push(message);
                continue;
            }
            if (message is MqttChannelSubscribeCommand mqttSubscription)
            {
                MqttChannelSubscribe pubsubMessage = new MqttChannelSubscribe(mqttSubscription.Topic);
                this.dispatcher.PushAll(pubsubMessage);
                string Topic = message.Topic;
                if (!this.subscriptionChannels.ContainsKey(Topic))
                {
                    this.subscriptionChannels[Topic] = new List<Channel<IMqttChannelMessage>>();
                }
                this.subscriptionChannels[Topic].Add(message.Channel);
                continue;
            }
            if (message is MqttChannelUnsubscribeCommand mqttUnsubscription)
            {
                MqttChannelUnsubscribe pubsubMessage = new MqttChannelUnsubscribe(mqttUnsubscription.Topic);
                this.dispatcher.PushAll(pubsubMessage);
                string Topic = message.Topic;
                if(this.subscriptionChannels.ContainsKey(Topic))
                {
                    this.subscriptionChannels[Topic].Remove(message.Channel);
                }
                continue;
            }
        }
    }

    /**
     * demux messages from mqtt client pool to specific channels based on topic
     * a list of channels is maintained for each topic
     */
    internal async Task MessageDemuxRoutine(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            IMqttChannelMessage message = await channel.Reader.ReadAsync(ct);
            string Topic = message.Topic;
            if (!this.subscriptionChannels.ContainsKey(Topic)) continue;
            foreach (Channel<IMqttChannelMessage> channel in this.subscriptionChannels[Topic])
            {
                await channel.Writer.WriteAsync(message);
            }
        }
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}