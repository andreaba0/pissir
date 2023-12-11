using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;

namespace MQTTConcurrent;

public class MQTTnetConcurrent : IMQTTnetConcurrent, IDisposable {
    private readonly ConnectionData cData;
    private Dictionary<string, Channel<MqttChannelMessage>> subscribeChannels;
    private Channel<IMqttChannelBus> demuxSendChannel;
    private RoundRobinDispatcher dispatcher;

    public MQTTnetConcurrent(string connectionString) {
        this.cData = ConnectionData.Parse(connectionString);
        this.subscribeChannels = new Dictionary<string, Channel<MqttChannelMessage>>();
        this.dispatcher = new RoundRobinDispatcher(cData.poolSize);
    }

#if TEST
    public MQTTnetConcurrent(ConnectionData cData, RoundRobinDispatcher dispatcher) {
        this.cData = cData;
        this.dispatcher = dispatcher;
        this.subscribeChannels = new Dictionary<string, Channel<MqttChannelMessage>>();
    }
#endif

    public Task RunAsync() {
        CancellationTokenSource cts = new CancellationTokenSource();
        MqttClientConcurrent[] mqttClients = new MqttClientConcurrent[cData.poolSize];
        Task[] workers = new Task[cData.poolSize];
        for (int i = 0; i < cData.poolSize; i++) {
            mqttClients[i] = new MqttClientConcurrent(cData);
            workers[i] = Task.Factory.StartNew(async () => await mqttClients[i].RunClient(cts.Token), TaskCreationOptions.LongRunning).Unwrap();
        }
        Task.WaitAll(workers);
        return Task.CompletedTask;
    }

    public async Task PublishQueue(string topic, string message) {
        MqttChannelMessage mqttMessage = new MqttChannelMessage(
            topic, 
            message
        );
        await this.dispatcher.Push(mqttMessage);
    }

    public async Task SubscribeQueue(string topic, Channel<MqttChannelMessage> channel) {
        lock (this.subscribeChannels) {
            if (this.subscribeChannels.ContainsKey(topic)) {
                throw new MqttConcurrentException("Topic already subscribed");
            } else {
                this.subscribeChannels.Add(topic, channel);
                //TODO subscribe to topic in all clients
            }
        }
    }

    public async Task UnsubscribeQueue(string topic) {
        throw new NotImplementedException();
    }

    public void Dispose() {
        throw new NotImplementedException();
    }
}