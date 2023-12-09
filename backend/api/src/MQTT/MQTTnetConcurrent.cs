namespace MQTTConcurrent;

public class MQTTnetConcurrent : IMQTTnetConcurrent {
    private readonly ConnectionData cData;
    private Dictionary<string, Channel> subscribeChannels;

    public MQTTnetConcurrent(string connectionString) {
        this.cData = ConnectionData.Parse(connectionString);
        this.channels = new Dictionary<string, Channel>();
    }

    public Task<int> RunAsync() {
        Task[] workers = new Task[cData.poolSize];
        for (int i = 0; i < cData.poolSize; i++) {
            workers[i] = Task.Factory.StartNew(async () => await RunClient(), TaskCreationOptions.LongRunning).Unwrap();
        }
    }

    internal Task RunClient() {
        MqttClientConcurrent mqttClient = new MqttClientConcurrent(ref readonly cData);
        
    }

    public void AddTopicSubscription(string topic, Channel channel) {
        throw new NotImplementedException();
    }

    internal bool SendToChannel(string topic, string message) {
        lock (this.subscribeChannels) {
            if (this.subscribeChannels.ContainsKey(topic)) {
                this.subscribeChannels[topic].Send(message);
                return true;
            }
            return false;
        }
    }

    public void Subscribe(string topic) {
        throw new NotImplementedException();
    }
}