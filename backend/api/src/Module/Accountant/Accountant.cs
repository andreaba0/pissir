using MQTTConcurrent;
using MQTTConcurrent.Message;

using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

using System;
using System.Data.Common;
using System.Threading.Channels;

using Types;

namespace Module.Accountant;
internal class TransactionDataObject
{
    public string data_type { get; set; }
    public Dictionary<string, object> data { get; set; }
    public static void Log(TransactionDataObject data)
    {
        foreach (KeyValuePair<string, object> kvp in data.data)
        {
            Console.WriteLine($"{kvp.Key} {kvp.Value}");
        }
    }
}
internal class TransactionObject
{
    public string transaction_id { get; set; }
    public string data_type { get; set; }
    public dynamic data { get; set; }
}

public sealed class Accountant : IDisposable
{
    private readonly DbDataSource _dbDataSource;
    private readonly Channel<IMqttBusPacket> _outgoingChannel;
    private readonly Channel<IMqttChannelMessage> _incomingChannel;
    private readonly string _topic = "transaction/signup/user";
    public Accountant(
        DbDataSource dbDataSource,
        Channel<IMqttBusPacket> outgoingChannel
    )
    {
        this._dbDataSource = dbDataSource;
        this._outgoingChannel = outgoingChannel;
        this._incomingChannel = Channel.CreateUnbounded<IMqttChannelMessage>();
    }

    public async Task RunAsync(CancellationToken cts = default)
    {
        IMqttBusPacket subscribePacket = new MqttChannelSubscribeCommand(_topic, this._incomingChannel);
        await this._outgoingChannel.Writer.WriteAsync(subscribePacket);
        Task[] workers = new Task[2];
        workers[0] = Task.Run(async () => await DispatchIncomingMessageRoutine(cts));
        workers[1] = Task.Run(async () => await DispatchIncomingMessageRoutine(cts));
        await Task.WhenAll(workers);
    }

    public async Task<int> DispatchIncomingMessageRoutine(CancellationToken cts = default)
    {
        Console.WriteLine("Dispatching incoming message");
        while (!cts.IsCancellationRequested)
        {
            IMqttChannelMessage message = await this._incomingChannel.Reader.ReadAsync(cts);
            try
            {
                TransactionObject transaction = JsonSerializer.Deserialize<TransactionObject>(message.Payload);
                Console.WriteLine(transaction.data_type);
                Console.WriteLine(transaction.data.GetProperty("Name"));
                await this._outgoingChannel.Writer.WriteAsync(new MqttChannelMessage(
                    "transaction/commit/signup/user",
                    transaction.transaction_id
                ));
                Console.WriteLine("Dispatched");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        return 0;
    }

    public async Task DisposeAsync()
    {
        IMqttBusPacket unsubscribePacket = new MqttChannelUnsubscribeCommand(_topic, this._incomingChannel);
        await this._outgoingChannel.Writer.WriteAsync(unsubscribePacket);
    }

    public void Dispose()
    {
        this.DisposeAsync().Wait();
    }
}