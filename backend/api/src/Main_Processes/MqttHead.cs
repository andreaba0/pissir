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
using System.Text.RegularExpressions;

namespace Main_Processes;

/// <summary>
/// The scope of this class is to store in database the data received from the MQTT broker
/// Data includes temperature, humidity and actuator status
/// </summary>
public class TopicSchema {
    public enum Type {
        Temperature,
        Humidity,
        Actuator
    }
    public readonly Type type;
    public TopicSchema(Type type) {
        this.type = type;
    }
    public static TopicSchema? Parse(string topicSchema) {
        Regex regex = new Regex(@"backend\/measure\/(?<object>[a-z]+)(\/(?<type>[a-z]+))?$");
        Match match = regex.Match(topicSchema);
        if (match.Success) {
            string objectName = match.Groups["object"].Value;
            string typeName = match.Groups["type"].Value;
            if (objectName == "sensor"&&typeName=="tmp") {
                return new TopicSchema(Type.Temperature);
            }
            if (objectName == "sensor"&&typeName=="umdty") {
                return new TopicSchema(Type.Humidity);
            }
            if (objectName == "actuator"&&typeName=="") {
                return new TopicSchema(Type.Actuator);
            }
        }
        return null;
    }

    public static bool ShouldBeRouted(string topic) {
        return Parse(topic) != null;
    }
}

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
            Console.WriteLine("Message received in head routine");
            //process the message
            if (!(message is MqttChannelMessage)) {
                Console.WriteLine("A malformed message was received");
            }
            //cast the message to a MqttChannelMessage
            MqttChannelMessage? mqttMessage = message as MqttChannelMessage;

            TopicSchema? result = TopicSchema.Parse(mqttMessage.Topic);
            if (result == null) {
                Console.WriteLine("A malformed topic was received");
                continue;
            }
        }

        return 0;
    }
}