using System.Data.Common;
using System.Data;
using System.Threading;
using System.Security.Cryptography;
using Npgsql;
using NpgsqlTypes;

using Utility;
using Middleware;
using Routes;

using Module.KeyManager;

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace Main_Processes;

public class WebServer
{
    private readonly DbDataSource _dbDataSource;
    private readonly RemoteManager _remoteManager;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly string _localIssuer;
    private readonly string _localAudience;
    private readonly string _boundAddress;

    public WebServer(
        DbDataSource dbDataSource, 
        RemoteManager remoteManager,
        IDateTimeProvider dateTimeProvider,
        string localIssuer,
        string localAudience,
        string boundAddress
    ) {
        _dbDataSource = dbDataSource;
        _remoteManager = remoteManager;
        _dateTimeProvider = dateTimeProvider;
        _localIssuer = localIssuer;
        _localAudience = localAudience;
        _boundAddress = boundAddress;
    }

    public async Task<int> RunAsync(CancellationToken cancellationToken = default)
    {
        //create a web server with WebApplication builder that listen on port wit manuel route handling
        //var builder = WebApplication.CreateBuilder();
        //var configuration = builder.Configuration;
        //var app = builder.Build();

        /*
        Create object app as webserver that listen on port 5000 and reject default launchSettings.json
        */
        var app = WebApplication.Create(args: new string[] { "--urls", _boundAddress });


        app.MapGet("/ping", async context =>
        {
            await context.Response.WriteAsync("pong");
        });

        app.MapPost("/water/limit", async context => {
            try {
                WaterLimit.PostWaterLimit(
                    context.Request.Headers,
                    context.Request.Body,
                    _dbDataSource,
                    _dateTimeProvider,
                    _remoteManager
                ).Wait();
                context.Response.StatusCode = 200;
                await context.Response.WriteAsync("Water limit set");
            } catch(WaterLimitException e) {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync(e.Message);
            } catch(JsonException e) {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync(e.Message);
            } catch(Exception e) {
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync(e.Message);
            }
        });

        Task webServerTask = app.RunAsync();

        Console.WriteLine("WebServer started");

        await webServerTask;

        Console.WriteLine("WebServer stopped");

        return 0;
    }
}