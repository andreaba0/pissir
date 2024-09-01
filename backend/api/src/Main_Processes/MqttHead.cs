using MQTTConcurrent.Message;
using Module;
using System.Threading.Channels;
using System.Threading;
using System;
using System.Threading.Tasks;
using MQTTConcurrent;
using Npgsql;
using System.Collections.Generic;
using System.Data.Common;
using System.Data;
using NpgsqlTypes;

namespace Main_Processes;

public class MqttHeadRoutine {
    private Channel<IMqttChannelMessage> receivingChannel;
    private Channel<IMqttBusPacket> sendingChannel;
    private readonly DbDataSource dbDataSource;
    private string topic_schema;
    public MqttHeadRoutine(
        string topic_schema,
        DbDataSource dbDataSource,
        Channel<IMqttBusPacket> sendingChannel
    ) {
        this.receivingChannel = Channel.CreateUnbounded<IMqttChannelMessage>();
        this.sendingChannel = sendingChannel;
        this.topic_schema = topic_schema;
    }
    public async Task<int> RunAsync(CancellationToken tk) {
        // TODO: subscribe to topics
        // TODO: loop through messages


        //create a subscribe message for the topic
        var subscribeMessage = new MqttChannelSubscribeCommand(this.topic_schema, this.receivingChannel);

        //send the subscribe message
        await this.sendingChannel.Writer.WriteAsync(subscribeMessage);

        //loop through messages
        while (!tk.IsCancellationRequested) {
            //read a message from the receiving channel
            Console.WriteLine("Waiting for message");
            var message = await this.receivingChannel.Reader.ReadAsync(tk);
            //process the message
            if (message is MqttChannelMessage mqttMessage) {
                Console.WriteLine(mqttMessage.Payload);
            }
        }

        return 0;
    }
}