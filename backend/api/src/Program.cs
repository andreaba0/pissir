using System.Threading;
using System.Threading.Channels;
using MQTTConcurrent;

using Npgsql;

using System.Data.Common;

using Module.JsonWebToken;
using Module.KeyManager;
using Utility;
using Module.WebServer;

class Program
{
    internal static string GetProperty(IConfiguration configuration, string key)
    {
        string systemEnvKey = $"DOTNET_ENV_{key.Replace(":", "_").ToUpper()}";
        string? systemEnvValue = Environment.GetEnvironmentVariable(systemEnvKey);
        if (systemEnvValue != null)
        {
            return systemEnvValue;
        }
        string? value = configuration[key];
        if (value == null)
        {
#if LOAD_APPSETTINGS
            throw new Exception($"Missing configuration key: {key} in appsettings.json");
#else
            throw new Exception($"Missing configuration key: {key} in environment variables");
#endif
        }
        return value;
    }
    public static int Main(string[] args)
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
#if LOAD_APPSETTINGS
            .AddJsonFile("appsettings.json", optional:true, reloadOnChange:true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional:false, reloadOnChange:true);
#else
            .AddEnvironmentVariables();
#endif
        var configuration = builder.Build();

        string mqttHost = GetProperty(configuration, "mqtt:host");
        string mqttPort = GetProperty(configuration, "mqtt:port");
        string mqttUsername = GetProperty(configuration, "mqtt:user");
        string mqttPassword = GetProperty(configuration, "mqtt:password");
        string mqttPoolSize = GetProperty(configuration, "mqtt:poolSize");
        string mqttPerClientCapacity = GetProperty(configuration, "mqtt:perClientCapacity");

        string postgresHost = GetProperty(configuration, "database:host");
        string postgresPort = GetProperty(configuration, "database:port");
        string postgresDatabaseName = GetProperty(configuration, "database:name");
        string postgresUsername = GetProperty(configuration, "database:user");
        string postgresPassword = GetProperty(configuration, "database:password");
        string backendAuthUri = GetProperty(configuration, "auth:uri");

        string pissirIssuer = GetProperty(configuration, "pissir:iss");
        string pissirAudience = GetProperty(configuration, "pissir:aud");

        string webserverBound = GetProperty(configuration, "webserver:bound");

        CancellationTokenSource cts = new CancellationTokenSource();

        //Shared thread safe instances
        DbDataSource dataSource = NpgsqlDataSource.Create($"host={postgresHost};port={postgresPort};database={postgresDatabaseName};username={postgresUsername};password={postgresPassword};Pooling=true");
        MQTTnetConcurrent mqttPool = new MQTTnetConcurrent(
            $"host={mqttHost};port={mqttPort};username={mqttUsername};password={mqttPassword};poolSize={mqttPoolSize};perClientCapacity={mqttPerClientCapacity}",
            "api"
        );

        //Shared channel used to send data to mqtt client pool
        Channel<IMqttBusPacket> mqttChannel = mqttPool.GetSharedInputChannel();

        HttpClient httpClient = new HttpClient();
        httpClient.Timeout = TimeSpan.FromSeconds(5);

        RemoteManager remoteKeyManager = new RemoteManager(
            $"{backendAuthUri}/.well-known/oauth/openid/jwks",
            httpClient,
            pissirIssuer
        );

        WebServer webServer = new WebServer(
            dataSource,
            remoteKeyManager,
            new DateTimeProvider(),
            pissirIssuer,
            pissirAudience,
            webserverBound
        );

        Task keyManagerTask = Task.Factory.StartNew(() => {
            try {
                remoteKeyManager.RunAsync(cts.Token).Wait();
            } catch (AggregateException e) {
                e.Handle((ex) => {
                    if (ex is OperationCanceledException) {
                        Console.WriteLine("KeyManagerTask cancelled");
                        return true;
                    }
                    return false;
                });
            }
        }, TaskCreationOptions.LongRunning);

        Task mqttTask = Task.Factory.StartNew(() => {
            /*try {
                mqttPool.RunAsync(cts.Token).Wait();
            } catch (AggregateException e) {
                foreach (var ex in e.InnerExceptions) {
                    Console.WriteLine(ex.GetType().Name);
                    Console.WriteLine(ex.Message);
                }
            }*/
            mqttPool.RunAsync(cts.Token).Wait();
        }, TaskCreationOptions.LongRunning);
        Task webServerTask = Task.Factory.StartNew(() => webServer.RunAsync(cts.Token).Wait(), TaskCreationOptions.LongRunning);

        Console.CancelKeyPress += (sender, e) =>
        {
            Console.WriteLine("Ctrl+C pressed, cancelling tasks...");
            cts.Cancel();
            e.Cancel = true;
        };

        mqttTask.Wait();
        keyManagerTask.Wait();
        webServerTask.Wait();

        Console.WriteLine("Exiting...");
        return 0;
    }
}