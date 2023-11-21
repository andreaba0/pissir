using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using MQTTnet.Packets;
using MQTTnet.Formatter;
using MQTTnet.Exceptions;

using Npgsql;

using System.Text.Json;
using System.Threading.Tasks;
using System.Threading;
using System.Threading.Channels;

public class Accountant
{
    public int runServer()
    {
        var channelSQL = Channel.CreateUnbounded<User>(
            new UnboundedChannelOptions
            {
                SingleReader = false,
                SingleWriter = true
            }
        );
        var channelPush = Channel.CreateUnbounded<string>(
            new UnboundedChannelOptions
            {
                SingleReader = false,
                SingleWriter = false
            }
        );
        Task[] workers = new Task[10];

        //dispatch messages from mqtt broker to workers
        workers[0] = Task.Factory.StartNew(async () => await PubsubService(channelSQL), TaskCreationOptions.LongRunning).Unwrap();

        //every worker insert data into database
        workers[1] = Task.Factory.StartNew(async () => await Worker(1, channelSQL, channelPush), TaskCreationOptions.LongRunning).Unwrap();
        workers[2] = Task.Factory.StartNew(async () => await Worker(2, channelSQL, channelPush), TaskCreationOptions.LongRunning).Unwrap();
        workers[3] = Task.Factory.StartNew(async () => await Worker(3, channelSQL, channelPush), TaskCreationOptions.LongRunning).Unwrap();
        workers[4] = Task.Factory.StartNew(async () => await Worker(4, channelSQL, channelPush), TaskCreationOptions.LongRunning).Unwrap();
        workers[5] = Task.Factory.StartNew(async () => await Worker(5, channelSQL, channelPush), TaskCreationOptions.LongRunning).Unwrap();
        workers[6] = Task.Factory.StartNew(async () => await Worker(6, channelSQL, channelPush), TaskCreationOptions.LongRunning).Unwrap();

        //every worker receive the id of the inserted user into the database from channelPush and send message back to mqtt broker
        workers[7] = Task.Factory.StartNew(async () => await CommitWorker(1, channelPush), TaskCreationOptions.LongRunning).Unwrap();
        workers[8] = Task.Factory.StartNew(async () => await CommitWorker(2, channelPush), TaskCreationOptions.LongRunning).Unwrap();
        workers[9] = Task.Factory.StartNew(async () => await CommitWorker(3, channelPush), TaskCreationOptions.LongRunning).Unwrap();
        Task res = Task.Factory.ContinueWhenAll(workers, completedTasks =>
        {
            Console.WriteLine("Accounting done");
        });
        res.Wait();
        return 0;
    }

    private async Task Worker(int id, Channel<User> channelSQL, Channel<string> channelPush)
    {
        Console.WriteLine($"Worker {id}: Started");
        var connectionString = "Host=192.168.178.152;Username=andrea;Password=Andrea3000!;Database=pissir;Pooling=false;Timeout=5;Port=5432";
        await using var dataSource = NpgsqlDataSource.Create(connectionString);

        NpgsqlConnection? conn = null;

        while (true)
        {
            var user = await channelSQL.Reader.ReadAsync();
            UserParser usr = new UserParser(user);

            Console.WriteLine($"Worker {id}: Received user");

            Console.WriteLine($"Worker {id}: Connecting to database");

            try
            {
                if (conn==null || conn.State != System.Data.ConnectionState.Open)
                {
                    conn = await dataSource.OpenConnectionAsync(CancellationToken.None);
                }
                Console.Write($"Worker {id}: ");
                Console.WriteLine($"Connected to database");
                //conn?.Dispose();
            }
            catch (NpgsqlException ex)
            {
                conn?.Dispose();
                Console.WriteLine(ex);
                channelSQL.Writer.TryWrite(user);
                continue;
            }

            channelPush.Writer.TryWrite(user.id);
        }
    }

    private async Task CommitWorker(int workerId, Channel<string> channel)
    {
        List<string> pendingIds = new List<string>();

        var mqttFactory = new MqttFactory();
        var mqttClient = mqttFactory.CreateMqttClient();
        var mqttClientOptions = new MqttClientOptionsBuilder()
            .WithTcpServer("127.0.0.1")
            .WithProtocolVersion(MqttProtocolVersion.V500)
            .Build();
        while (true)
        {
            var id = await channel.Reader.ReadAsync();
            Console.Write($"CommitWorker {workerId}: ");
            Console.WriteLine($"User: {id}");
            try
            {
                if (!mqttClient.IsConnected)
                {
                    await mqttClient.ConnectAsync(mqttClientOptions);
                }
                var mqttApplicationMessage = new MqttApplicationMessageBuilder()
                    .WithTopic("api/commit/signup")
                    .WithPayload(id)
                    .Build();
                await mqttClient.PublishAsync(mqttApplicationMessage, CancellationToken.None);
            }
            catch (Exception)
            {
                Console.WriteLine($"Connection to broker lost from CommitWorker {workerId}");
                pendingIds.Add(id);
                continue;
            }
        }
    }

    private async Task<int> PubsubService(Channel<User> channel)
    {
        int exitCode;
        do
        {
            try
            {
                await ConnectToBroker(channel);
                exitCode = 0;
            }
            catch (Exception)
            {
                exitCode = 1;
            }
            finally
            {
                Console.WriteLine("Reconnecting to broker...");
                Thread.Sleep(5000);
            }
        } while (exitCode != 0);
        return exitCode;
    }

    private async Task ConnectToBroker(Channel<User> channel)
    {
        var mqttClientOptions = new MqttClientOptionsBuilder()
        .WithTcpServer($"127.0.0.1")
        .WithProtocolVersion(MqttProtocolVersion.V500)
        .Build();

        var mqttFactory = new MqttFactory();
        var mqttClient = mqttFactory.CreateMqttClient();

        mqttClient.ApplicationMessageReceivedAsync += e => processMessage(e, channel);

        mqttClient.DisconnectedAsync += async e =>
        {
            await Task.CompletedTask;
        };


        try
        {
            using (var timeoutToken = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
            {
                await mqttClient.ConnectAsync(mqttClientOptions, timeoutToken.Token);
            }
        }
        catch (Exception)
        {
            throw;
        }

        var mqttSubscribeOptions = new MqttClientSubscribeOptionsBuilder()
            .WithTopicFilter("$share/api/user/signup")
            .Build();

        await mqttClient.SubscribeAsync(mqttSubscribeOptions);
        while (true)
        {
            if (mqttClient == null)
            {
                throw new Exception("mqttClient is null");
            }
            if (!mqttClient.IsConnected)
            {
                throw new Exception("Connection to broker lost");
            }
        }
    }

    private async Task processMessage(MqttApplicationMessageReceivedEventArgs e, Channel<User> channel)
    {
        //parse payload
        var payload = System.Text.Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);
        var payloadJson = JsonSerializer.Deserialize<User>(payload);
        UserParser userParser = new UserParser(payloadJson);
        channel.Writer.TryWrite(payloadJson);
    }
}

class User
{
    public string? id { get; set; }
    public string? codice_fiscale { get; set; }
    public string? name { get; set; }
    public string? surname { get; set; }
    public string? company { get; set; }
    public string? role { get; set; }
}

class UserParser : User
{
    private User? user;
    public UserParser(User user)
    {
        this.user = user;
    }

    public Role getRole()
    {
        if (this.user?.role == "GSI")
        {
            return Role.GSI;
        }
        else if (this.user?.role == "UA")
        {
            return Role.UA;
        }
        else
        {
            return Role.UNKNOW;
        }
    }

    public bool isValidId()
    {
        if (Ulid.TryParse(this.user?.id, out var id))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}

enum Role
{
    GSI,
    UA,
    UNKNOW
}