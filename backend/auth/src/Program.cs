using System.Threading;
using System.Threading.Channels;

using Npgsql;

using Utility;

using System.Data.Common;
using Module.WebServer;

using Module.KeyManager;

class Program
{
    internal static string GetProperty(IConfiguration configuration, string key) {
        string systemEnvKey = $"DOTNET_ENV_{key.Replace(":", "_").ToUpper()}";
        string? systemEnvValue = Environment.GetEnvironmentVariable(systemEnvKey);
        if(systemEnvValue != null) {
            return systemEnvValue;
        }
        string? value = configuration[key];
        if(value == null) {
            throw new Exception($"Missing configuration key: {key}");
        }
        return value;
    }
    public static int Main(string[] args)
    {
        Console.WriteLine($"Environment: {Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}");
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional:true, reloadOnChange:true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional:false, reloadOnChange:true)
            .AddEnvironmentVariables();
        var configuration = builder.Build();

        string postgresHost = GetProperty(configuration, "database:host");
        string postgresPort = GetProperty(configuration, "database:port");
        string postgresDatabaseName = GetProperty(configuration, "database:database");
        string postgresUsername = GetProperty(configuration, "database:username");
        string postgresPassword = GetProperty(configuration, "database:password");

        CancellationTokenSource cts = new CancellationTokenSource();

        Console.CancelKeyPress += (sender, a) => {
            Console.WriteLine("Exiting...");
            a.Cancel = true;
            cts.Cancel();
        };

        //Shared thread safe instances
        DbDataSource dataSource = NpgsqlDataSource.Create($"host={postgresHost};port={postgresPort};database={postgresDatabaseName};username={postgresUsername};password={postgresPassword};Pooling=true;");

        HttpClient httpClient = new HttpClient();
        httpClient.Timeout = TimeSpan.FromSeconds(5);

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
        Task webServerTask = Task.Factory.StartNew(() => webServer.RunAsync(cts.Token), TaskCreationOptions.LongRunning).Unwrap();

        //wait for webserver to exit
        webServerTask.Wait();

        Console.WriteLine("Exiting...");
        return 0;
    }
}