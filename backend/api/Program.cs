using System.Threading;

var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.Development.json");
var configuration = builder.Build();

Thread WebserverThread;
Thread AuditingThread;
Thread AccountingThread;

WebserverThread = new Thread(new ThreadStart(() => {
    Webserver webserver = new Webserver(
        configuration["database:api:host"],
        configuration["database:api:port"],
        configuration["database:api:database"],
        configuration["database:api:user"],
        configuration["database:api:password"]
    );
    webserver.runServer();
}));

AuditingThread = new Thread(new ThreadStart(() => {
    Audit auditing = new Audit(
        configuration["database:api:host"],
        configuration["database:api:port"],
        configuration["database:api:database"],
        configuration["database:api:user"],
        configuration["database:api:password"]
    );
    auditing.runServer();
}));

AccountingThread = new Thread(new ThreadStart(() => {
    Accountant accounting = new Accountant(
        configuration["database:api:host"],
        configuration["database:api:port"],
        configuration["database:api:database"],
        configuration["database:api:user"],
        configuration["database:api:password"]
    );
    accounting.runServer();
}));

WebserverThread.Start();
AuditingThread.Start();
AccountingThread.Start();

WebserverThread.Join();
AuditingThread.Join();
AccountingThread.Join();