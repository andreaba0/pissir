using System.Threading;

var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.Development.json");
var configuration = builder.Build();

Thread WebserverThread;
Thread AuditingThread;
Thread AccountingThread;

/*WebserverThread = new Thread(new ThreadStart(() => {
    Webserver webserver = new Webserver(
        configuration["database:api:host"],
        configuration["database:api:port"],
        configuration["database:api:database"],
        configuration["database:api:user"],
        configuration["database:api:password"]
    );
    webserver.runServer();
}));*/

/*AuditingThread = new Thread(new ThreadStart(async () => {
    Audit auditing = new Audit(
        configuration["database:api:host"],
        configuration["database:api:port"],
        configuration["database:api:database"],
        configuration["database:api:user"],
        configuration["database:api:password"]
    );
    await auditing.runServer();
}));*/

Accountant accountantProcess = new Accountant(
    configuration["mqtt:host"],
    $"Host={configuration["database:api:host"]};Port={configuration["database:api:port"]};Database={configuration["database:api:database"]};Username={configuration["database:api:user"]};Password={configuration["database:api:password"]}"
);

//AccountingThread = new Thread(new ThreadStart(async () => await accountantProcess.runServer()));

//WebserverThread.Start();
//AuditingThread.Start();
//AccountingThread.Start();

//WebserverThread.Join();
//AuditingThread.Join();
//AccountingThread.Join();


Task accountingTask = Task.Run(() => accountantProcess.runServer());

// You can do other work here if needed

// Wait for the accounting task to complete before exiting

accountingTask.Wait();

System.Console.WriteLine("Hello World!");