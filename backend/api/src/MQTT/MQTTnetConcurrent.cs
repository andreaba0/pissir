namespace MQTTConcurrent;

public class MQTTnetConcurrent : IMQTTnetConcurrent, IDisposable {
    private readonly ConnectionData cData;
    private Dictionary<string, Channel> subscribeChannels;
    private Channel<string> demuxSendChannel;
    private RoundRobinDispatcher dispatcher;

    public MQTTnetConcurrent(string connectionString) {
        this.cData = ConnectionData.Parse(connectionString);
        this.subscribeChannels = new Dictionary<string, Channel>();
        this.dispatcher = new RoundRobinDispatcher(cData.poolSize);
    }

#if TEST
    public MQTTnetConcurrent(ConnectionData cData, RoundRobinDispatcher dispatcher) {
        this.cData = cData;
        this.dispatcher = dispatcher;
        this.subscribeChannels = new Dictionary<string, Channel>();
    }
#endif

    public Task RunAsync() {
        CancellationTokenSource cts = new CancellationTokenSource();
        MqttClientConcurrent[] mqttClients = new MqttClientConcurrent[cData.poolSize];
        Task[] workers = new Task[cData.poolSize];
        for (int i = 0; i < cData.poolSize; i++) {
            mqttClients[i] = new MqttClientConcurrent(ref readonly cData);
            workers[i] = Task.Factory.StartNew(async () => await mqttClients[i].RunClient(cts.Token), TaskCreationOptions.LongRunning).Unwrap();
        }
        Task.WaitAll(workers);
        return Task.CompletedTask;
    }

    internal bool PublishQueue(string topic, string message) {
        MqttChannelMessage mqttMessage = new MqttChannelMessage(
            topic, 
            message, 
            MqttChannelMessageType.MESSAGE
        );
        lock (this.subscribeChannels) {
            if (this.subscribeChannels.ContainsKey(topic)) {
                this.subscribeChannels[topic].Send(mqttMessage);
                return true;
            }
            return false;
        }
    }

    public void SubscribeQueue(string topic, Channel channel) {
        lock (this.subscribeChannels) {
            if (this.subscribeChannels.ContainsKey(topic)) {
                this.subscribeChannels[topic] = channel;
            } else {
                this.subscribeChannels.Add(topic, channel);
            }
        }
        this.dispatcher.Push(topic);
    }

    public void UnsubscribeQueue(string topic) {
        throw new NotImplementedException();
    }

    public void Dispose() {
        throw new NotImplementedException();
    }
}