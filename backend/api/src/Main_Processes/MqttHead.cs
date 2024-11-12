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
using Types;
using Utility;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text;

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
            int res = await ProcessMessage(result, mqttMessage);
            if (res != 0) {
                Console.WriteLine("An error occurred while processing the message");
            }
        }

        return 0;
    }

    internal async Task<int> ProcessMessage(
        TopicSchema topicSchema,
        MqttChannelMessage mqttMessage
    ) {
        string data = mqttMessage.Payload;
        string[] parts = data.Split('.');
        string info = Utility.Utility.Base64URLDecode(parts[0]);
        string signature = Utility.Utility.Base64URLDecode(parts[1]);
        try {
            MqttMessageIot message = JsonSerializer.Deserialize<MqttMessageIot>(info, new JsonSerializerOptions {
                PropertyNameCaseInsensitive = true,
                IncludeFields = true
            });
            if (message == null) {
                return 1;
            }
            string vat_number = message.vat_number;
            DbConnection connection = this.dbDataSource.OpenConnection();
            DbCommand command = this.dbDataSource.CreateCommand();
            command.CommandText = "select secret_key from secret_key where company_vat_number = $1";
            command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, vat_number));
            DbDataReader reader = command.ExecuteReader();
            if (!reader.HasRows) {
                return 1;
            }
            reader.Read();
            string secret_key = reader.GetString(0);
            reader.Close();
            connection.Close();
            if(Utility.Utility.HmacSha256(secret_key, info) != signature) {
                return 1;
            }
            if (message.type == "actuator") {
                return await AddActuatorData(info, message.log_timestamp, message.obj_id, message.field_id);
            }
            if (message.type == "sensor") {
                return await AddSensorData(info, message.log_timestamp, message.obj_id, message.field_id, topicSchema);
            }
            return 1;
        } catch (Exception e) {
            return 1;
        }
    }

    internal Task<int> AddActuatorData(
        string data,
        long log_timestamp,
        string object_id,
        string field_id
    ) {
        DbConnection connection = this.dbDataSource.OpenConnection();
        try {
            Actuator actuator = JsonSerializer.Deserialize<Actuator>(data, new JsonSerializerOptions {
                PropertyNameCaseInsensitive = true,
                IncludeFields = true
            });
            if (actuator == null) {
                return Task.FromResult(1);
            }
            DbCommand command = this.dbDataSource.CreateCommand();
            command.CommandText = "BEGIN TRANSACTION";
            command.ExecuteNonQuery();
            command.CommandText = $@"
                insert into object_logger (id, company_chosen_id, object_type, farm_field_id) values
                ($1, $2, $3, $4)
                on conflict (id) do nothing
            ";
            command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, object_id));
            command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, object_id));
            command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.Object, CustomDbType.ObjectType.ACTUATOR));
            command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, field_id));
            command.ExecuteNonQuery();
            command.Parameters.Clear();

            command.CommandText = $@"
                insert into actuator_log (object_id, object_type, log_time, is_active, water_used) values
                ($1, $2, $3, $4)
            ";
            command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, object_id));
            command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.Object, CustomDbType.ObjectType.ACTUATOR));
            command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.DateTime, new DateTime(log_timestamp)));
            command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.Boolean, true));
            command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.Single, actuator.water_used));
            command.ExecuteNonQuery();

            command.CommandText = "COMMIT TRANSACTION";
            command.ExecuteNonQuery();
            return Task.FromResult(0);
        }
         catch (Exception e) {
            DbCommand command = this.dbDataSource.CreateCommand();
            command.CommandText = "ROLLBACK TRANSACTION";
            command.ExecuteNonQuery();
            return Task.FromResult(1);
        }
    }

    internal Task<int> AddSensorData(
        string data,
        long log_timestamp,
        string object_id,
        string field_id,
        TopicSchema topicSchema
    ) {
        DbConnection connection = this.dbDataSource.OpenConnection();
        try {
            Sensor sensor = JsonSerializer.Deserialize<Sensor>(data, new JsonSerializerOptions {
                PropertyNameCaseInsensitive = true,
                IncludeFields = true
            });
            if (sensor == null) {
                return Task.FromResult(1);
            }
            DbCommand command = this.dbDataSource.CreateCommand();
            command.CommandText = "BEGIN TRANSACTION";
            command.ExecuteNonQuery();
            command.CommandText = $@"
                insert into object_logger (id, company_chosen_id, object_type, farm_field_id) values
                ($1, $2, $3, $4)
                on conflict (id) do nothing
            ";
            command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, object_id));
            command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, object_id));
            if(topicSchema.type == TopicSchema.Type.Temperature) {
                command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.Object, CustomDbType.ObjectType.TMP));
            } else if(topicSchema.type == TopicSchema.Type.Humidity) {
                command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.Object, CustomDbType.ObjectType.UMDTY));
            } else {
                return Task.FromResult(1);
            }
            command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, field_id));
            command.ExecuteNonQuery();
            command.Parameters.Clear();

            float value = float.Parse(sensor.value);
            string table = topicSchema.type switch {
                TopicSchema.Type.Temperature => "tmp_ssensor_log",
                TopicSchema.Type.Humidity => "umdty_sensor_log",
                _ => "unknown"
            };

            string column = topicSchema.type switch {
                TopicSchema.Type.Temperature => "tmp",
                TopicSchema.Type.Humidity => "umdty",
                _ => "unknown"
            };

            command.CommandText = $@"
                insert into {table} (object_id, object_type, log_time, {column}) values
                ($1, $2, $3, $4)
            ";
            command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, object_id));
            if(topicSchema.type == TopicSchema.Type.Temperature) {
                command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.Object, CustomDbType.ObjectType.TMP));
            } else if(topicSchema.type == TopicSchema.Type.Humidity) {
                command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.Object, CustomDbType.ObjectType.UMDTY));
            }
            command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.DateTime, new DateTime(log_timestamp)));
            command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.Single, value));
            command.ExecuteNonQuery();

            command.CommandText = "COMMIT TRANSACTION";
            command.ExecuteNonQuery();
            return Task.FromResult(0);
        }
         catch (Exception e) {
            DbCommand command = this.dbDataSource.CreateCommand();
            command.CommandText = "ROLLBACK TRANSACTION";
            command.ExecuteNonQuery();
            return Task.FromResult(1);
        }
    }
}