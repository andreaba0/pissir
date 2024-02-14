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
        string localIssuer = GetProperty(configuration, "local_issuer");

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

        string initialDate = Environment.GetEnvironmentVariable("INITIAL_DATE") ?? DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
        Regex dateRegex = new Regex("^(?<day>[0-9]{2})/(?<month>[0-9]{2})/(?<year>[0-9]{4}) (?<hour>[0-9]{2}):(?<minute>[0-9]{2}):(?<second>[0-9]{2})$");
        Match match = dateRegex.Match(initialDate);
        if(!match.Success) {
            throw new Exception("Invalid date format");
        }
        DateTime startDate = new DateTime(
            int.Parse(match.Groups["year"].Value),
            int.Parse(match.Groups["month"].Value),
            int.Parse(match.Groups["day"].Value),
            int.Parse(match.Groups["hour"].Value),
            int.Parse(match.Groups["minute"].Value),
            int.Parse(match.Groups["second"].Value)
        );

        IDateTimeProvider dateTimeProvider = new DateTimeProvider(
            startDate
        );
        Console.WriteLine($"Start date: {startDate}");

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
            localIssuer
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