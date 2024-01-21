using System.Threading;
using System.Threading.Channels;
using MQTTConcurrent;

using Npgsql;

using System.Data.Common;

using Module.Accountant;
using Module.JsonWebToken;
using Utility;

class Program
{
    internal static string GetProperty(IConfiguration configuration, string key) {
        string? value = configuration[key];
        if(value == null) {
            throw new Exception($"Missing configuration key: {key}");
        }
        return value;
    }
    public static int Main(string[] args)
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
#if DEVELOPMENT
            .AddJsonFile("appsettings.Development.json");
#else
            .AddJsonFile("appsettings.json");
#endif
        var configuration = builder.Build();

        string mqttHost = GetProperty(configuration, "mqtt:host");
        string mqttPort = GetProperty(configuration, "mqtt:port");
        string mqttUsername = GetProperty(configuration, "mqtt:username");
        string mqttPassword = GetProperty(configuration, "mqtt:password");
        string mqttPoolSize = GetProperty(configuration, "mqtt:poolSize");
        string mqttPerClientCapacity = GetProperty(configuration, "mqtt:perClientCapacity");

        string postgresHost = GetProperty(configuration, "database:host");
        string postgresPort = GetProperty(configuration, "database:port");
        string postgresDatabaseName = GetProperty(configuration, "database:database");
        string postgresUsername = GetProperty(configuration, "database:username");
        string postgresPassword = GetProperty(configuration, "database:password");
        string backendAuthUri = GetProperty(configuration, "auth:uri");

        CancellationTokenSource cts = new CancellationTokenSource();

        //Shared thread safe instances
        DbDataSource dataSource = NpgsqlDataSource.Create($"host={postgresHost};port={postgresPort};database={postgresDatabaseName};username={postgresUsername};password={postgresPassword};Pooling=true");
        MQTTnetConcurrent mqttPool = new MQTTnetConcurrent(
            $"host={mqttHost};port={mqttPort};username={mqttUsername};password={mqttPassword};poolSize={mqttPoolSize};perClientCapacity={mqttPerClientCapacity}",
            "api"
        );

        //Shared channel used to send data to mqtt client pool
        Channel<IMqttBusPacket> mqttChannel = mqttPool.GetSharedInputChannel();

        KeyService keyManager = new KeyService(
            backendAuthUri,
            new Fetch()
        );


        Accountant accountant = new Accountant(
            dataSource,
            mqttChannel
        );

        WebServer webServer = new WebServer(
            dataSource,
            new JwtControl(
                new ClockCustom(),
                new Fetch(),
                backendAuthUri
            ),
            new ClockCustom(),
            keyManager
        );

        Task mqttTask = Task.Factory.StartNew(() => mqttPool.RunAsync(
            cts.Token
        ), TaskCreationOptions.LongRunning);
        Task accountantTask = Task.Factory.StartNew(() => accountant.RunAsync(
            cts.Token
        ), TaskCreationOptions.LongRunning);
        Task webServerTask = Task.Factory.StartNew(() => webServer.runServer(), TaskCreationOptions.LongRunning);
        Task keyManagerTask = Task.Factory.StartNew(() => keyManager.RunAsync(
            cts.Token
        ), TaskCreationOptions.LongRunning);

        mqttTask.Wait();
        accountantTask.Wait();
        webServerTask.Wait();
        keyManagerTask.Wait();

        Console.WriteLine("Exiting...");
        return 0;
    }
}