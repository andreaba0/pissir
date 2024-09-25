using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Channels;

using MQTTConcurrent.Message;

namespace MQTTConcurrent;

/*

    This class aims to to create a pool of MQTT clients with the intent of speeding up the processing of messages
    This custom library based on MQTTnet leverages on version 5 of the MQTT protocol that allows to 
    receive messages belonging to a group of clients in a round-robin fashion
    Each client of this pool subscribe to the same client group so only one client will receive a message
    Messages from classes that implement this library are dispatched to one of the clients in the pool in a round-robin fashion
    Messages from classes will be sent to the pool of clients through a shared channel
    Messages include both simple messages and subscribe/unsubscribe commands
    Subscribe command to topic must be simple topic path because each client in the pool will make a subscription
    with mqtt server using a $share prefix

*/


public class MQTTnetConcurrent : IMQTTnetConcurrent, IDisposable
{
    private readonly ConnectionData cData;
    //private Dictionary<string, List<Channel<IMqttChannelMessage>>> subscriptionChannels;
    private Dictionary<string, int> subscriptionsToChannels;
    private List<Channel<IMqttChannelMessage>> channelSubscriptions;
    private Channel<IMqttBusPacket> demuxChannel;
    private RoundRobinDispatcher dispatcher;
    private Channel<IMqttBusPacket> sharedInputChannel;
    private string ServerTopic;

    private class DelegateItem
    {
        public MQTTnetConcurrent.TopicShouldBeRouted Method { get; }
        public Channel<IMqttChannelMessage> Channel { get; }

        public DelegateItem(MQTTnetConcurrent.TopicShouldBeRouted ShouldSend, Channel<IMqttChannelMessage> Channel)
        {
            this.Method = ShouldSend;
            this.Channel = Channel;
        }
    }

    public MQTTnetConcurrent(
        string connectionString,
        string serverTopic
        )
    {
        this.cData = ConnectionData.Parse(connectionString);
        this.subscriptionsToChannels = new Dictionary<string, int>();
        this.channelSubscriptions = new List<Channel<IMqttChannelMessage>>();
        this.ServerTopic = serverTopic;

        if (cData.perClientCapacity.HasValue)
        {
            this.dispatcher = new RoundRobinDispatcher(cData.poolSize, cData.perClientCapacity.Value);
        }
        else
        {
            this.dispatcher = new RoundRobinDispatcher(cData.poolSize);
        }

        this.demuxChannel = Channel.CreateUnbounded<IMqttBusPacket>();
        if (cData.perClientCapacity.HasValue)
        {
            this.sharedInputChannel = Channel.CreateBounded<IMqttBusPacket>(cData.perClientCapacity.Value);
        }
        else
        {
            this.sharedInputChannel = Channel.CreateUnbounded<IMqttBusPacket>();
        }
    }

    public delegate bool TopicShouldBeRouted(string topic);

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
    protected bool lookForNestedException(Exception e, Type target)
    {
        if (e == null) return false;
        Console.WriteLine(e.GetType());
        if (e.GetType() == target) return true;
        if (e is AggregateException ae)
        {
            foreach (Exception inner in ae.InnerExceptions)
            {
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
            workers[currentId] = Task.Factory.StartNew(() =>
            {
                try
                {
                    mqttClients[currentId].RunClient(ct).Wait();
                }
                catch (AggregateException e)
                {
                    e.Handle((ex) =>
                    {
                        if (ex is OperationCanceledException)
                        {
                            Console.WriteLine("MqttClientTask cancelled");
                            return true;
                        }
                        return false;
                    });
                }
            }, TaskCreationOptions.LongRunning);
        }
        workers[cData.poolSize] = Task.Factory.StartNew(() =>
        {
            try
            {
                MessageDispatcherRoutine(ct).Wait();
            }
            catch (AggregateException e)
            {
                e.Handle((ex) =>
                {
                    if (ex is OperationCanceledException)
                    {
                        Console.WriteLine("MessageDispatcherTask cancelled");
                        return true;
                    }
                    return false;
                });
            }
        }, TaskCreationOptions.LongRunning);
        workers[cData.poolSize + 1] = Task.Factory.StartNew(() =>
        {
            try
            {
                MessageDemuxRoutine(ct).Wait();
            }
            catch (AggregateException e)
            {
                e.Handle((ex) =>
                {
                    if (ex is OperationCanceledException)
                    {
                        Console.WriteLine("MessageDemuxTask cancelled");
                        return true;
                    }
                    return false;
                });
            }
        }, TaskCreationOptions.LongRunning);

        try
        {
            //Console.WriteLine(typeof(OperationCanceledException));
            Console.WriteLine("MQTTnetConcurrent started");
            await Task.WhenAll(workers);
        }
        catch (Exception)
        {
            //log the state of the tasks
            foreach (Task worker in workers)
            {
                Console.WriteLine(worker.Status);
                if (lookForNestedException(worker.Exception, typeof(OperationCanceledException)))
                {
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
            //if it is a simple message, then it is dispatched to a client
            //if it is a subscribe/unsubscribe command, then it is dispatched to all clients
            //if it is a simple message or a delegate/undelegate command, then it is pushed to demux channel
            IMqttBusPacket message = await this.sharedInputChannel.Reader.ReadAsync(ct);
            if (message is MqttChannelMessage mqttMessage)
            {
                this.dispatcher.Push(mqttMessage);
                continue;
            }
            if (message is MqttChannelSubscribeCommand mqttSubscription)
            {
                Console.WriteLine($"1 Subscribing to {mqttSubscription.Topic}");
                string idempotentTopic = "$share/" + this.ServerTopic + "/" + mqttSubscription.Topic;
                string topic = mqttSubscription.Topic;
                Console.WriteLine($"2 Subscribing to {idempotentTopic}");
                MqttChannelSubscribe pubsubMessage = new MqttChannelSubscribe(idempotentTopic);
                if (!this.subscriptionsToChannels.ContainsKey(topic))
                {
                    this.subscriptionsToChannels[topic] = 1;
                    this.dispatcher.PushAll(pubsubMessage);
                }
                else
                {
                    this.subscriptionsToChannels[topic]++;
                }
                await this.demuxChannel.Writer.WriteAsync(mqttSubscription);
                continue;
            }
            if (message is MqttChannelUnsubscribeCommand mqttUnsubscription)
            {
                string Topic = mqttUnsubscription.Topic;
                if (this.subscriptionsToChannels.ContainsKey(Topic))
                {
                    if (this.subscriptionsToChannels[Topic] == 0)
                    {
                        MqttChannelUnsubscribe pubsubMessage2 = new MqttChannelUnsubscribe("$share/" + this.ServerTopic + "/" + Topic);
                        this.dispatcher.PushAll(pubsubMessage2);
                        this.subscriptionsToChannels.Remove(Topic);
                    }
                    this.subscriptionsToChannels[Topic]--;
                }
                await this.demuxChannel.Writer.WriteAsync(mqttUnsubscription);
                continue;
            }
        }
        if (ct.IsCancellationRequested)
        {
            ct.ThrowIfCancellationRequested();
            Console.WriteLine("MessageDispatcherRoutine stopped");
        }
    }

    /**
     * demux messages from mqtt client pool to specific channels based on topic
     * a list of channels is maintained for each topic
     * This routine is the only one that can manipulate the subscriptionsToChannels dictionary
     */
    internal async Task MessageDemuxRoutine(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            IMqttBusPacket message = await this.demuxChannel.Reader.ReadAsync(ct);
            if (message is MqttChannelMessage mqttMessage)
            {
                for (int i = 0; i < this.channelSubscriptions.Count; i++)
                {
                    this.channelSubscriptions[i].Writer.WriteAsync(mqttMessage);
                }
                continue;
            }
            if (message is MqttChannelSubscribeCommand mqttSubscription)
            {
                this.channelSubscriptions.Add(mqttSubscription.ResponseChannel);
                continue;
            }
            if (message is MqttChannelUnsubscribeCommand mqttUnsubscription)
            {
                this.channelSubscriptions.RemoveAll(delegate(Channel<IMqttChannelMessage> channel)
                {
                    return channel == mqttUnsubscription.ResponseChannel;
                });
                continue;
            }
        }
        if (ct.IsCancellationRequested)
        {
            ct.ThrowIfCancellationRequested();
            Console.WriteLine("MessageDemuxRoutine stopped");
        }
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}