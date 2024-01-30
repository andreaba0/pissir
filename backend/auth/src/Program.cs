using System.Threading;
using System.Threading.Channels;
using MQTTConcurrent;

using Npgsql;

using Utility;

using System.Data.Common;
using Module.WebServer;

using Module.KeyManager;

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

        string openIdCertsGoogle = GetProperty(configuration, "openid:certs:google");
        string openIdCertsFacebook = GetProperty(configuration, "openid:certs:facebook");

        CancellationTokenSource cts = new CancellationTokenSource();

        //Shared thread safe instances
        DbDataSource dataSource = NpgsqlDataSource.Create($"host={postgresHost};port={postgresPort};database={postgresDatabaseName};username={postgresUsername};password={postgresPassword};Pooling=true;");
        MQTTnetConcurrent mqttPool = new MQTTnetConcurrent(
            $"host={mqttHost};port={mqttPort};username={mqttUsername};password={mqttPassword};poolSize={mqttPoolSize};perClientCapacity={mqttPerClientCapacity}",
            "auth"
        );

        HttpClient httpClient = new HttpClient();
        httpClient.Timeout = TimeSpan.FromSeconds(5);

        //Shared channel used to send data to mqtt client pool
        Channel<IMqttBusPacket> mqttChannel = mqttPool.GetSharedInputChannel();

        ISharedStorage dbHasChanged = new SharedStorage((bool)false);

        RemoteJwksHub remoteJwksHub = new RemoteJwksHub(
            httpClient
        );
        
        WebServer webServer = new WebServer(
            dataSource,
            remoteJwksHub,
            new QueryKeyService(
                dataSource,
                remoteJwksHub
            )
        );

        Task mqttTask = Task.Factory.StartNew(() => mqttPool.RunAsync(
            cts.Token
        ), TaskCreationOptions.LongRunning);
        Task webServerTask = Task.Factory.StartNew(() => webServer.RunAsync(cts.Token), TaskCreationOptions.LongRunning);

        mqttTask.Wait();
        webServerTask.Wait();

        Console.WriteLine("Exiting...");
        return 0;
    }
}