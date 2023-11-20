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
    private MqttClientOptions mqttClientOptions;
    private string connectionString;
    private NpgsqlDataSource dataSource;
    private IMqttClient mqttClient;
    public Accountant(
        string mqtt_host,
        string postgres_config
    )
    {
        System.Console.WriteLine("Accounting...");
        this.mqttClientOptions = new MqttClientOptionsBuilder().WithTcpServer($"{mqtt_host}").WithProtocolVersion(MqttProtocolVersion.V500).Build();
    }

    public int runServer()
    {
        var channel = Channel.CreateUnbounded<User>(
            new UnboundedChannelOptions
            {
                SingleReader = false,
                SingleWriter = true
            }
        );
        Task[] workers = new Task[6];
        workers[0] = Task.Run(async () => await PubsubService(channel));
        for (int i = 1; i < workers.Length; i++)
        {
            int id = i;
            workers[i] = Task.Run(async () => await Worker(id, channel));
        }
        Task.WaitAll(workers);
        Console.WriteLine("Accounting done");
        return 0;
    }

    private async Task<int> Worker(int id, Channel<User> channel)
    {
        while (true)
        {
            var user = await channel.Reader.ReadAsync();
            Console.Write($"Worker {id}: ");
            Console.WriteLine($"User: {user.id}");
        }
        return 0;
    }

    private async Task<int> PubsubService(Channel<User> channel)
    {
        int exitCode;
        do
        {
            exitCode = await ConnectToBroker(channel);
            Thread.Sleep(5000);
        } while (exitCode != 0);
        return exitCode;
    }

    private async Task ConnectToPostgres()
    {
        await using var dataSource = NpgsqlDataSource.Create(this.connectionString);
        this.dataSource = dataSource;
    }

    private async Task<int> ConnectToBroker(Channel<User> channel)
    {
        var mqttClientOptions = new MqttClientOptionsBuilder().WithTcpServer($"127.0.0.1").WithProtocolVersion(MqttProtocolVersion.V500).Build();

        var mqttFactory = new MqttFactory();
        var mqttClient = mqttFactory.CreateMqttClient();

        mqttClient.ApplicationMessageReceivedAsync += e => processMessage(e, channel);

        mqttClient.DisconnectedAsync += async e =>
        {
            Console.WriteLine("### DISCONNECTED FROM SERVER ###");
            Task.FromResult(0);
        };


        try
        {
            using (var timeoutToken = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
            {
                await mqttClient.ConnectAsync(mqttClientOptions, timeoutToken.Token);
            }
            System.Console.WriteLine("Connected to broker");
        }
        catch (Exception e)
        {
            return 1;
        }

        var mqttSubscribeOptions = new MqttClientSubscribeOptionsBuilder()
            .WithTopicFilter("$share/api/user/signup")
            .Build();

        await mqttClient.SubscribeAsync(mqttSubscribeOptions);
        while (true)
        {
            if (this.mqttClient == null || !this.mqttClient.IsConnected)
            {
                System.Console.WriteLine("Connection to broker lost");
                this.mqttClient.Dispose();
                return 1;
            }
        }
        return 0;
    }

    private async Task processMessage(MqttApplicationMessageReceivedEventArgs e, Channel<User> channel)
    {
        Console.WriteLine("### RECEIVED APPLICATION MESSAGE ###");
        Console.WriteLine(e.ApplicationMessage.Topic);

        //parse payload
        var payload = System.Text.Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
        var payloadJson = JsonSerializer.Deserialize<User>(payload);

        UserParser userParser = new UserParser(payloadJson);

        channel.Writer.TryWrite(payloadJson);

        Console.WriteLine($"User role: {userParser.getRole()}");
        Console.WriteLine($"Valid id: {userParser.isValidId()}");

        return;
    }
}

class User
{
    public string id { get; set; }
    public string codice_fiscale { get; set; }
    public string name { get; set; }
    public string surname { get; set; }
    public string company { get; set; }
    public string role { get; set; }
}

class UserParser : User
{
    private User user;
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