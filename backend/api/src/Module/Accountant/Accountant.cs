using MQTTConcurrent;
using MQTTConcurrent.Message;

namespace Module.Accountant;

public sealed class Accountant : IDisposable {
    private readonly DbDataSource _dbDataSource;
    private readonly Channel<IMqttBusPacket> _outgoingChannel;
    private readonly Channel<IMqttChannelMessage> _incomingChannel;
    private readonly string _topic = "user/signup";
    public Accountant(
        DbDataSource dbDataSource,
        Channel<IMqttChannelMessage> incomingChannel
    ) {
        this._dbDataSource = dbDataSource;
        this._outgoingChannel = new Channel<IMqttBusPacket>();
        this._incomingChannel = incomingChannel;
        IMqttBusPacket subscribePacket = new MqttChannelSubscribeCommand(_topic, this._incomingChannel);
    }

    public async Task RunAsync(CancellationToken cts=default) {
        Task[] workers = new Task[2];
        workers[0] = Task.Run(async () => await DispatchIncomingMessageRoutine(cts));
        workers[1] = Task.Run(async () => await DispatchIncomingMessageRoutine(cts));
        await Task.WhenAll(workers);
    }

    public async Task DispatchIncomingMessageRoutine(CancellationToken cts=default) {
        while(!cts.IsCancellationRequested) {
            IMqttChannelMessage message = await this._incomingChannel.Reader.ReadAsync(cts);
            
        }
    }

    public async Task DisposeAsync() {
        IMqttBusPacket unsubscribePacket = new MqttChannelUnsubscribeCommand(_topic, this._incomingChannel);
        await this._outgoingChannel.Writer.WriteAsync(unsubscribePacket);
    }
}