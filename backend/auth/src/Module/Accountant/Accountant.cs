using System;
using System.Threading;
using System.Threading.Channels;
using MQTTConcurrent;
using MQTTConcurrent.Message;
using System.Data.Common;
using System.Data;
using System.Text.Json;
using System.Text.Json.Serialization;

using Npgsql;

using Utility;

namespace Module.Accountant;

internal class TransactionObject
{
    public string transaction_id { get; set; }
    public string data { get; set; }
}

internal class UpdateInterval
{
    private int _seconds;
    private int _dbCallCount;
    public UpdateInterval()
    {
        //convert DateTime.Now to seconds
        int actualTime = (int)(DateTimeOffset.Now.ToUnixTimeSeconds());
        int seconds = actualTime % 30;
        _seconds = actualTime - seconds;
        _dbCallCount = 0;
    }
    public int Update()
    {
        Console.WriteLine($"{_seconds} {_dbCallCount}");
        int actualTime = (int)(DateTimeOffset.Now.ToUnixTimeSeconds());
        int seconds = actualTime % 30;
        int interval = actualTime - seconds;
        if (interval == _seconds)
        {
            _dbCallCount++;
            return _dbCallCount;
        }
        else
        {
            _seconds = actualTime - seconds;
            _dbCallCount = 0;
            return _dbCallCount;
        }
    }
}

public class Accountant
{
    //TODO thread to periodically pull open transactions from database and push to pubsub
    private int currentRTT; //seconds
    private object _lockCurrentRTT = new object();
    private Channel<IMqttBusPacket> mqttChannel;
    private Channel<IMqttChannelMessage> commitChannel;
    private DbDataSource _dbDataSource;
    private ISharedStorage _dbHasChanged;
    private UpdateInterval _updateInterval = new UpdateInterval();
    public Accountant(
        DbDataSource dataSource,
        Channel<IMqttBusPacket> mqttChannel,
        ISharedStorage dbHasChanged
    )
    {
        this.mqttChannel = mqttChannel;
        this.commitChannel = Channel.CreateUnbounded<IMqttChannelMessage>();
        this.currentRTT = 8;
        this._dbDataSource = dataSource;
        this._dbHasChanged = dbHasChanged;
    }

    public async Task<int> RunAsync(CancellationToken tk)
    {
        Task[] tasks = new Task[2];
        tasks[0] = Task.Factory.StartNew(() => TransactionDispatcherRoutine(tk));
        tasks[1] = Task.Factory.StartNew(() => TransactionCommitRoutine(tk));
        //when any of the task finish, restart it

        return 0;
    }

    public async Task<int> TransactionCommitRoutine(CancellationToken tk)
    {
        mqttChannel.Writer.TryWrite(new MqttChannelSubscribeCommand("transaction/signup/user", commitChannel));
        while (!tk.IsCancellationRequested)
        {
            IMqttChannelMessage message = await commitChannel.Reader.ReadAsync(tk);
        }
        return 0;
    }

    public async Task<int> TransactionDispatcherRoutine(CancellationToken tk)
    {
        (string id, string bodyData)[] transactions = new (string, string)[5];
        int index = 0;
        while (!tk.IsCancellationRequested)
        {
            int updateCount = _updateInterval.Update();
            if (updateCount > 0 && index != 5 && !(bool)_dbHasChanged.GetValue())
            {
                await Task.Delay(5 * 1000, tk);
                continue;
            }
            transactions = new (string, string)[5];
            index = 0;
            DbConnection connection = await _dbDataSource.OpenConnectionAsync();
            try
            {
                //TODO open db connection, init transaction
                //connection = 

                {
                    Console.WriteLine("begin transaction");
                    var command = connection.CreateCommand();
                    command.CommandText = "BEGIN TRANSACTION ISOLATION LEVEL REPEATABLE READ";
                    await command.ExecuteNonQueryAsync();
                }

                {
                    //set local lock timeout
                    var command = connection.CreateCommand();
                    command.CommandText = "SET LOCAL lock_timeout = '6s'";
                    await command.ExecuteNonQueryAsync();
                }

                {
                    //select 5 pending transactions that are not updated for more than currentRTT seconds
                    var command = connection.CreateCommand();
                    command.CommandText = @"
                        SELECT id, body_data
                        FROM pending_transaction
                        WHERE updated_at < now() - INTERVAL '$1 seconds'
                        FOR UPDATE SKIP LOCKED
                        LIMIT 5
                    ";
                    //create parameter
                    var rttParameter = DbProviderFactories.GetFactory(connection).CreateParameter();
                    rttParameter.Value = currentRTT;
                    rttParameter.DbType = DbType.Int32;
                    command.Parameters.Add(rttParameter);
                    var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        var id = reader.GetGuid(0).ToString();
                        var bodyData = reader.GetString(1);
                        transactions[index++] = (id, bodyData);
                    }
                    if (index == 0)
                    {
                        await reader.CloseAsync();
                        var commitCommand = connection.CreateCommand();
                        commitCommand.CommandText = "COMMIT";
                        await commitCommand.ExecuteNonQueryAsync();
                        await connection.CloseAsync();
                        await Task.Delay(currentRTT * 1000, tk);
                        continue;
                    }
                    await reader.CloseAsync();
                }

                {
                    //for each id in transactions, update its updated_at field in database
                    var command = connection.CreateCommand();
                    command.CommandText = @"
                        UPDATE pending_transaction
                        SET updated_at = NOW()
                        WHERE id = $1
                    ";
                    for (int i = 0; i < index; i++)
                    {
                        command.Parameters.Clear();
                        //create parameter
                        var idParameter = DbProviderFactories.GetFactory(connection).CreateParameter();
                        idParameter.Value = new Guid(transactions[i].id);
                        idParameter.DbType = DbType.Guid;
                        command.Parameters.Add(idParameter);
                        await command.ExecuteNonQueryAsync();
                    }
                }

                {
                    Console.WriteLine("commit transaction");
                    //commit transaction
                    var command = connection.CreateCommand();
                    command.CommandText = "COMMIT";
                    await command.ExecuteNonQueryAsync();
                }


                //TODO push to pubsub
                for (int i = 0; i < index; i++)
                {
                    TransactionObject transactionObject = new TransactionObject();
                    transactionObject.transaction_id = transactions[i].id;
                    transactionObject.data = transactions[i].bodyData;
                    string json = JsonSerializer.Serialize(transactionObject);
                    mqttChannel.Writer.TryWrite(new MqttChannelMessage("transaction/signup/user", json));
                }

            }
            catch (NpgsqlException wrapper)
            {
                if (wrapper.InnerException is IOException)
                {
                    Console.WriteLine("Database connection lost");
                    await Task.Delay(1000, tk);
                    continue;
                }
                else
                {
                    //rollback transaction
                    //Console.WriteLine("rollback transaction");
                    Console.WriteLine(wrapper);
                    var command = connection.CreateCommand();
                    command.CommandText = "ROLLBACK";
                    await command.ExecuteNonQueryAsync();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                index = 0;
                await connection.CloseAsync();
            }
            //await Task.Delay(new Random().Next(5, 30) * 1000, tk);
            await Task.Delay(5 * 1000, tk);
        }
        return 0;
    }
}