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

    //look for nested exception of type target, loop also through aggregate exceptions
    protected bool lookForNestedException(Exception e, Type target) {
        if (e == null) return false;
        Console.WriteLine(e.GetType());
        if (e.GetType() == target) return true;
        if (e is AggregateException ae) {
            foreach (Exception inner in ae.InnerExceptions) {
                if (lookForNestedException(inner, target)) return true;
            }
        }
        return lookForNestedException(e.InnerException, target);
    }

    public async Task RunAsync(CancellationToken ct)
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
            workers[currentId] = Task.Factory.StartNew(() => {
                try {
                    mqttClients[currentId].RunClient(ct).Wait();
                } catch (AggregateException e) {
                    e.Handle((ex) => {
                        if (ex is OperationCanceledException) {
                            Console.WriteLine("MqttClientTask cancelled");
                            return true;
                        }
                        return false;
                    });
                }
            }, TaskCreationOptions.LongRunning);
        }
        workers[cData.poolSize] = Task.Factory.StartNew(() => {
            try {
                MessageDispatcherRoutine(ct).Wait();
            } catch (AggregateException e) {
                e.Handle((ex) => {
                    if (ex is OperationCanceledException) {
                        Console.WriteLine("MessageDispatcherTask cancelled");
                        return true;
                    }
                    return false;
                });
            }
        }, TaskCreationOptions.LongRunning);
        workers[cData.poolSize + 1] = Task.Factory.StartNew(() => {
            try {
                MessageDemuxRoutine(ct).Wait();
            } catch (AggregateException e) {
                e.Handle((ex) => {
                    if (ex is OperationCanceledException) {
                        Console.WriteLine("MessageDemuxTask cancelled");
                        return true;
                    }
                    return false;
                });
            }
        }, TaskCreationOptions.LongRunning);

        try {
            //Console.WriteLine(typeof(OperationCanceledException));
            Console.WriteLine("MQTTnetConcurrent started");
            await Task.WhenAll(workers);
        } catch (Exception) {
            //log the state of the tasks
            foreach (Task worker in workers)
            {
                Console.WriteLine(worker.Status);
                if (lookForNestedException(worker.Exception, typeof(OperationCanceledException))) {
                    Console.WriteLine("Task cancelled");
                }
            }
        }

        Console.WriteLine("MQTTnetConcurrent stopped");

        return;
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
                Console.WriteLine($"1 Subscribing to {mqttSubscription.Topic}");
                string idempotentTopic = "$share/"+this.ServerTopic+"/" + mqttSubscription.Topic;
                Console.WriteLine($"2 Subscribing to {idempotentTopic}");
                MqttChannelSubscribe pubsubMessage = new MqttChannelSubscribe(idempotentTopic);
                this.dispatcher.PushAll(pubsubMessage);
                if (!this.subscriptionChannels.ContainsKey(idempotentTopic))
                {
                    this.subscriptionChannels[idempotentTopic] = new List<Channel<IMqttChannelMessage>>();
                }
                this.subscriptionChannels[idempotentTopic].Add(mqttSubscription.Channel);
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
        if (ct.IsCancellationRequested) {
            ct.ThrowIfCancellationRequested();
            Console.WriteLine("MessageDispatcherRoutine stopped");
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
        if (ct.IsCancellationRequested) {
            ct.ThrowIfCancellationRequested();
            Console.WriteLine("MessageDemuxRoutine stopped");
        }
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}