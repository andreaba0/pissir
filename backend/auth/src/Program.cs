using System.Threading;
using System.Threading.Channels;
using System.Text.RegularExpressions;

using Npgsql;

using Utility;

using System.Data.Common;
using Module.WebServer;

using Module.KeyManager;

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
            throw new Exception($"Missing configuration key: {key} in environment variables");
        }
        return value;
    }
    public static int Main(string[] args)
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddEnvironmentVariables();
        var configuration = builder.Build();

        string postgresHost = GetProperty(configuration, "database:host");
        string postgresPort = GetProperty(configuration, "database:port");
        string postgresDatabaseName = GetProperty(configuration, "database:name");
        string postgresUsername = GetProperty(configuration, "database:user");
        string postgresPassword = GetProperty(configuration, "database:password");
        string issuer = GetProperty(configuration, "pissir:iss");
        string audience = GetProperty(configuration, "pissir:aud");
        string boundAddress = GetProperty(configuration, "webserver:bound");

        CancellationTokenSource cts = new CancellationTokenSource();

        Console.CancelKeyPress += (sender, a) =>
        {
            Console.WriteLine("Exiting...");
            a.Cancel = true;
            cts.Cancel();
        };

        string? initialDate = Environment.GetEnvironmentVariable("INITIAL_DATE");
        IDateTimeProvider dateTimeProvider = new DateTimeProvider();
        if (initialDate != null) {
            dateTimeProvider = DateTimeProvider.parse(initialDate);
        }

        //Shared thread safe instances
        DbDataSource dataSource = NpgsqlDataSource.Create($"host={postgresHost};port={postgresPort};database={postgresDatabaseName};username={postgresUsername};password={postgresPassword};Pooling=true;");

        HttpClient httpClient = new HttpClient();
        httpClient.Timeout = TimeSpan.FromSeconds(5);

        ISharedStorage dbHasChanged = new SharedStorage((bool)false);

        RemoteJwksHub remoteJwksHub = new RemoteJwksHub(
            httpClient
        );
        remoteJwksHub.RunAsync(cts.Token);

        LocalManager localManager = new LocalManager(
            dataSource
        );

        QueryKeyService queryKeyServie = new QueryKeyService(
            dataSource,
            remoteJwksHub
        );

        WebServer webServer = new WebServer(
            dataSource,
            remoteJwksHub,
            localManager,
            dateTimeProvider,
            issuer,
            audience,
            boundAddress
        );
        Task webServerTask = Task.Factory.StartNew(() => webServer.RunAsync(cts.Token), TaskCreationOptions.LongRunning).Unwrap();
        Task queryKeyServiceTask = Task.Factory.StartNew(() => queryKeyServie.RunAsync(cts.Token), TaskCreationOptions.LongRunning).Unwrap();

        //wait for webserver to exit
        webServerTask.Wait();
        queryKeyServiceTask.Wait();

        Console.WriteLine("Exiting...");
        return 0;
    }
}