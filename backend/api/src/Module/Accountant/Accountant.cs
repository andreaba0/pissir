using MQTTConcurrent;
using MQTTConcurrent.Message;

using System;
using System.Data.Common;
using System.Threading.Channels;

using Types;

namespace Module.Accountant;

public sealed class Accountant : IDisposable {
    private readonly DbDataSource _dbDataSource;
    private readonly Channel<IMqttBusPacket> _outgoingChannel;
    private readonly Channel<IMqttChannelMessage> _incomingChannel;
    private readonly string _topic = "user/signup";
    public Accountant(
        DbDataSource dbDataSource,
        Channel<IMqttBusPacket> outgoingChannel
    ) {
        this._dbDataSource = dbDataSource;
        this._outgoingChannel = outgoingChannel;
        this._incomingChannel = Channel.CreateUnbounded<IMqttChannelMessage>();
    }

    public async Task RunAsync(CancellationToken cts=default) {
        IMqttBusPacket subscribePacket = new MqttChannelSubscribeCommand(_topic, this._incomingChannel);
        await this._outgoingChannel.Writer.WriteAsync(subscribePacket);
        Task[] workers = new Task[2];
        workers[0] = Task.Run(async () => await DispatchIncomingMessageRoutine(cts));
        workers[1] = Task.Run(async () => await DispatchIncomingMessageRoutine(cts));
        await Task.WhenAll(workers);
    }

    public async Task DispatchIncomingMessageRoutine(CancellationToken cts=default) {
        while(!cts.IsCancellationRequested) {
            IMqttChannelMessage message = await this._incomingChannel.Reader.ReadAsync(cts);
            /*User user = User.ParseJsonEncoded(message.Payload);
            if(!user.Has(new User.Fields[] {
                User.Fields.ID, 
                User.Fields.EMAIL,
                User.Fields.NAME,
                User.Fields.SURNAME,
                User.Fields.ROLE
            })) continue;*/
        }
    }

    public async Task DisposeAsync() {
        IMqttBusPacket unsubscribePacket = new MqttChannelUnsubscribeCommand(_topic, this._incomingChannel);
        await this._outgoingChannel.Writer.WriteAsync(unsubscribePacket);
    }

    public void Dispose() {
        this.DisposeAsync().Wait();
    }
}