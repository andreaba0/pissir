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
        var app = WebApplication.Create(args: new string[] { "--urls", _boundAddress });


        // The following 2 endpoints are not part of the API specification.
        // They are used to test the web server

        app.MapGet("/ping", async context =>
        {
            await context.Response.WriteAsync("pong");
        });

        app.MapGet("/time", async context => {
            await context.Response.WriteAsync(_dateTimeProvider.Now.ToString());
        });



        // Here are the endpoints that are part of the API specification <api-definition/api.yaml>

        app.MapPost("/company/secret", async context => {
            try {
                string data = Secret.PostCompanySecret(
                    context.Request.Headers,
                    _dbDataSource,
                    _dateTimeProvider,
                    _remoteManager
                );
                context.Response.StatusCode = 200;
                await context.Response.WriteAsync(data);
            } catch(AuthenticationException e) {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync(e.Message);
            } catch(AuthorizationException e) {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync(e.Message);
            } catch(Exception e) {
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync(e.Message);
            }
        })

        app.MapGet("/crops", async context => {
            try {
                string data = Crops.GetCrops(
                    context.Request.Headers,
                    _dbDataSource,
                    _dateTimeProvider,
                    _remoteManager
                );
                context.Response.StatusCode = 200;
                await context.Response.WriteAsync(data);
            } catch(AuthenticationException e) {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync(e.Message);
            } catch(AuthorizationException e) {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync(e.Message);
            } catch(Exception e) {
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync(e.Message);
            }
        });

        app.MapGet("/irrigation", async context => {
            try {
                string data = Irrigation.GetIrrigations(
                    context.Request.Headers,
                    _dbDataSource,
                    _dateTimeProvider,
                    _remoteManager
                );
                context.Response.StatusCode = 200;
                await context.Response.WriteAsync(data);
            } catch(AuthenticationException e) {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync(e.Message);
            } catch(AuthorizationException e) {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync(e.Message);
            } catch(Exception e) {
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync(e.Message);
            }
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

        app.MapGet("/water/recommendation/{field_id}", async context => {
            await context.Response.WriteAsync("Water recommendation");
        });

        app.MapGet("/water/limit", async context => {
            await context.Response.WriteAsync("Water limit");
        });

        app.MapGet("/water/limit/all", async context => {
            try {
                string data = WaterLimitAll.GetWaterLimitAll(
                    context.Request.Headers,
                    _dbDataSource,
                    _dateTimeProvider,
                    _remoteManager
                );
                context.Response.StatusCode = 200;
                await context.Response.WriteAsync(data);
            } catch(AuthenticationException e) {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync(e.Message);
            } catch(AuthorizationException e) {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync(e.Message);
            } catch(Exception e) {
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync(e.Message);
            }
        });

        app.MapGet("/water/stock", async context => {
            await context.Response.WriteAsync("Water stock");
        });

        app.MapGet("/water/stock/{field_id}", async context => {
            await context.Response.WriteAsync("Water stock");
        });

        app.MapPost("/water/buy", async context => {
            try {
                WaterBuy.PostWaterBuy(
                    context.Request.Headers,
                    _dbDataSource,
                    _dateTimeProvider,
                    _remoteManager
                ).Wait();
                context.Response.StatusCode = 200;
                await context.Response.WriteAsync("Water bought");
            } catch(WaterBuyException e) {
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

        app.MapGet("/water/offer", async context => {
            await context.Response.WriteAsync("Water offer");
        });

        app.MapPost("/water/offer", async context => {
            await context.Response.WriteAsync("Water offer");
        });

        app.MapDelete("/water/offer/{offer_id}", async context => {
            await context.Response.WriteAsync("Water offer");
        });

        app.MapPatch("/water/offer/{offer_id}", async context => {
            await context.Response.WriteAsync("Water offer");
        });

        app.MapGet("/water/order", async context => {
            await context.Response.WriteAsync("Water order");
        });

        app.MapGet("/water/consumption", async context => {
            await context.Response.WriteAsync("Water consumption");
        });

        app.MapGet("/field/{field_id}", async context => {
            try {
                List<Field.GetData> data = Field.GetField(
                    context.Request.Headers,
                    _dbDataSource,
                    _dateTimeProvider,
                    _remoteManager
                );
                context.Response.StatusCode = 200;
                await context.Response.WriteAsJsonAsync(data);
            } catch(AuthorizationException e) {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync(e.Message);
            } catch(Exception e) {
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync(e.Message);
            }
        });

        app.MapPatch("/field/{field_id}", async context => {
            await context.Response.WriteAsync("Field");
        });

        app.MapGet("/field", async context => {
            try {
                string data = Fields.GetFields(
                    context.Request.Headers,
                    _dbDataSource,
                    _dateTimeProvider,
                    _remoteManager
                );
                context.Response.StatusCode = 200;
                await context.Response.WriteAsync(data);
            } catch(AuthenticationException e) {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync(e.Message);
            } catch(AuthorizationException e) {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync(e.Message);
            } catch(Exception e) {
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync(e.Message);
            }
        });

        app.MapPost("/field", async context => {
            try {
                Fields.PostField(
                    context.Request.Headers,
                    context.Request.Body,
                    _dbDataSource,
                    _dateTimeProvider,
                    _remoteManager
                ).Wait();
                context.Response.StatusCode = 200;
                await context.Response.WriteAsync("Field created");
            } catch(AuthorizationException e) {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync(e.Message);
            } catch(AuthenticationException e) {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync(e.Message);
            } catch(FieldException e) {
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

        app.MapGet("/object/sensor", async context => {
            try {
                List<SensorRoute.GetData> data = SensorRoute.GetSensorData(
                    context.Request.Headers,
                    _dbDataSource,
                    _dateTimeProvider,
                    _remoteManager
                );
                context.Response.StatusCode = 200;
                await context.Response.WriteAsJsonAsync(data);
            } catch(AuthorizationException e) {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync(e.Message);
            } catch(Exception e) {
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync(e.Message);
            }
        });

        app.MapGet("/object/actuator", async context => {
            try {
                List<ActuatorRoute.GetData> data = ActuatorRoute.GetActuatorData(
                    context.Request.Headers,
                    _dbDataSource,
                    _dateTimeProvider,
                    _remoteManager
                );
                context.Response.StatusCode = 200;
                await context.Response.WriteAsJsonAsync(data);
            } catch(AuthorizationException e) {
                context.Response.StatusCode = 403;
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