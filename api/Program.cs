using System;
using System.Threading;

public class Program
{

    private static readonly object transactionListLock = new object();
    private static List<Transaction> transactionList = new List<Transaction>();


    static void transactionGarbageCollector(int sleep)
    {
        Console.WriteLine("Transaction garbage collector started");
        Console.WriteLine("Sleep set to: " + sleep);
        while(true) {
            Console.WriteLine("Checking for expired transactions to free...");
            lock(transactionListLock) {
                if(transactionList.Count()>0) {

                }
            }
            Console.WriteLine("Check completed");
            Thread.Sleep(sleep);
        }
    }

    static void webServer(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
    static void Main(string[] args)
    {
        Thread transactionThread = new Thread(() => {
            transactionGarbageCollector(30000);
        });
        Thread webserverThread = new Thread(() => {
            webServer(args);
        });
        webserverThread.Start();
        transactionThread.Start();
        webserverThread.Join();
        transactionThread.Join();
    }
}


