using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;

using MQTTConcurrent.Message;

namespace MQTTConcurrent;

public class MQTTnetConcurrent : IMQTTnetConcurrent, IDisposable
{
    private readonly ConnectionData cData;
    private Dictionary<string, List<Channel<IMqttChannelMessage>>> subscriptionChannels;
    private Channel<IMqttChannelMessage> demuxChannel;
    private RoundRobinDispatcher dispatcher;
    private Channel<IMqttBusPacket> sharedInputChannel;
    private string ServerTopic;

    public MQTTnetConcurrent(
        string connectionString,
        string serverTopic
        )
    {
        this.cData = ConnectionData.Parse(connectionString);
        this.subscriptionChannels = new Dictionary<string, List<Channel<IMqttChannelMessage>>>();
        this.ServerTopic = serverTopic;

        if (cData.perClientCapacity.HasValue)
        {
            this.dispatcher = new RoundRobinDispatcher(cData.poolSize, cData.perClientCapacity.Value);
        }
        else
        {
            this.dispatcher = new RoundRobinDispatcher(cData.poolSize);
        }
        
        this.demuxChannel = Channel.CreateUnbounded<IMqttChannelMessage>();
        if (cData.perClientCapacity.HasValue)
        {
            this.sharedInputChannel = Channel.CreateBounded<IMqttBusPacket>(cData.perClientCapacity.Value);
        }
        else
        {
            this.sharedInputChannel = Channel.CreateUnbounded<IMqttBusPacket>();
        }
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

    public Channel<IMqttBusPacket> GetSharedInputChannel()
    {
        return this.sharedInputChannel;
    }

    public Task RunAsync(CancellationToken ct)
    {
        MqttClientConcurrent[] mqttClients = new MqttClientConcurrent[cData.poolSize];
        Task[] workers = new Task[cData.poolSize + 2];
        for (int i = 0; i < cData.poolSize; i++)
        {
            int currentId = i;
            Channel<IMqttBusPacket> clientChannel = this.dispatcher.GetChannel(currentId);
            mqttClients[currentId] = new MqttClientConcurrent(
                cData,
                clientChannel,
                this.demuxChannel
            );
            workers[currentId] = Task.Factory.StartNew(
                async () => await mqttClients[currentId].RunClient(ct), TaskCreationOptions.LongRunning
            ).Unwrap();
        }
        workers[cData.poolSize] = Task.Run(async () => await MessageDispatcherRoutine(ct));
        workers[cData.poolSize + 1] = Task.Run(async () => await MessageDemuxRoutine(ct));
        
        // when any of the workers is done write the id in the array to console
        // if ct is not cancelled yet, restart the worker
        Task.Factory.ContinueWhenAny(workers, (t) =>
        {
            int id = Array.IndexOf(workers, t);
            Console.WriteLine($"Worker {id} is done");
            if (!ct.IsCancellationRequested)
            {
                Console.WriteLine($"Restarting worker {id}");
                workers[id] = Task.Factory.StartNew(
                    async () => await mqttClients[id].RunClient(ct), TaskCreationOptions.LongRunning
                ).Unwrap();
            }
        });


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
                string Topic = ConnectionData.ParseTopic(mqttSubscription.Topic);
                if(Topic==string.Empty) continue;
                string idempotentTopic = "$share/"+this.ServerTopic+"/" + Topic;
                Console.WriteLine($"Subscribing to {idempotentTopic}");
                MqttChannelSubscribe pubsubMessage = new MqttChannelSubscribe(idempotentTopic);
                this.dispatcher.PushAll(pubsubMessage);
                if (!this.subscriptionChannels.ContainsKey(Topic))
                {
                    this.subscriptionChannels[Topic] = new List<Channel<IMqttChannelMessage>>();
                }
                this.subscriptionChannels[Topic].Add(mqttSubscription.Channel);
                continue;
            }
            if (message is MqttChannelUnsubscribeCommand mqttUnsubscription)
            {
                MqttChannelUnsubscribe pubsubMessage = new MqttChannelUnsubscribe(mqttUnsubscription.Topic);
                this.dispatcher.PushAll(pubsubMessage);
                string Topic = pubsubMessage.Topic;
                if(this.subscriptionChannels.ContainsKey(Topic))
                {
                    this.subscriptionChannels[Topic].Remove(mqttUnsubscription.Channel);
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
            IMqttChannelMessage message = await this.demuxChannel.Reader.ReadAsync(ct);
            Console.WriteLine(message.Payload);
            Console.WriteLine(message.Topic);
            string Topic = message.Topic;
            if (!this.subscriptionChannels.ContainsKey(Topic)) continue;
            foreach (Channel<IMqttChannelMessage> channel in this.subscriptionChannels[Topic])
            {
                Console.WriteLine("Writing to channel");
                await channel.Writer.WriteAsync(message);
            }
        }
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}