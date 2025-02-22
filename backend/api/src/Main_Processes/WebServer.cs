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

        app.MapGet("/health", async context => {
            context.Response.StatusCode = 204;
        });



        // Here are the endpoints that are part of the API specification <api-definition/api.yaml>

        app.MapGet("/resourcemanager/field", async context => {
            try {
                string data = ResourceManagerField.Get(
                    context.Request.Path,
                    context.Request.Method,
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

        app.MapGet("/resourcemanager/water/stock/{field_id}", async context => {
            try {
                ValueTask<string> func = ResourceManagerWaterStock.Get(
                    context.Request.Path,
                    context.Request.Method,
                    context.Request.Headers,
                    context.Request.RouteValues,
                    _dbDataSource,
                    _dateTimeProvider,
                    _remoteManager
                );
                string data = await func;
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

        app.MapPost("/company/secret", async context => {
            try {
                CompanySecret.PostResponse response = CompanySecret.Post(
                    context.Request.Cookies.ContainsKey("ApiToken") ? context.Request.Cookies["ApiToken"] : "",
                    _dbDataSource,
                    _dateTimeProvider,
                    _remoteManager
                );
                if(response.state==CompanySecret.KeyState.CREATED)
                context.Response.StatusCode = 201;
                else
                context.Response.StatusCode = 200;
                await context.Response.WriteAsync(response.secret_key);
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
                WaterLimit.Post(
                    context.Request.Headers,
                    context.Request.Body,
                    _dbDataSource,
                    _dateTimeProvider,
                    _remoteManager
                );
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
            try { 
                ValueTask<string> func = WaterRecommendationFieldId.Get(
                    context.Request.Headers,
                    context.Request.RouteValues,
                    _dbDataSource,
                    _dateTimeProvider,
                    _remoteManager
                );
                string data = await func;
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

        app.MapGet("/water/limit", async context => {
            try {
                ValueTask<string> func = WaterLimit.Get(
                    context.Request.Headers,
                    _dbDataSource,
                    _dateTimeProvider,
                    _remoteManager
                );
                string data = await func;
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
            try {
                ValueTask<string> func = WaterStock.Get(
                    context.Request.Headers,
                    _dbDataSource,
                    _dateTimeProvider,
                    _remoteManager
                );
                string data = await func;
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

        app.MapGet("/water/stock/{field_id}", async context => {
            try {
                ValueTask<string> func = WaterStockFieldId.Get(
                    context.Request.Headers,
                    context.Request.RouteValues,
                    _dbDataSource,
                    _dateTimeProvider,
                    _remoteManager
                );
                string data = await func;
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

        app.MapPost("/water/buy", async context => {
            try {
                WaterBuy.Post(
                    context.Request.Headers,
                    context.Request.Body,
                    _dbDataSource,
                    _dateTimeProvider,
                    _remoteManager
                ).Wait();
                context.Response.StatusCode = 200;
                await context.Response.WriteAsync("");
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
            try {
                ValueTask<string> func = WaterOffer.Get(
                    context.Request.Headers,
                    _dbDataSource,
                    _dateTimeProvider,
                    _remoteManager
                );
                string data = await func;
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

        app.MapPost("/water/offer", async context => {
            try {
                ValueTask<string> func = WaterOffer.Post(
                    context.Request.Headers,
                    context.Request.Body,
                    _dbDataSource,
                    _dateTimeProvider,
                    _remoteManager
                );
                string data = await func;
                context.Response.StatusCode = 200;
                await context.Response.WriteAsync(data);
            } catch(WaterOfferException e) {
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

        app.MapDelete("/water/offer/{offer_id}", async context => {
            try {
                ValueTask<WaterOfferOfferId.DeleteResponse> func = WaterOfferOfferId.Delete(
                    context.Request.Headers,
                    context.Request.RouteValues,
                    _dbDataSource,
                    _dateTimeProvider,
                    _remoteManager
                );
                WaterOfferOfferId.DeleteResponse r = await func;
                if(r == WaterOfferOfferId.DeleteResponse.OfferNotFound) {
                    context.Response.StatusCode = 404;
                    await context.Response.WriteAsync("Offer not found");
                } else if(r == WaterOfferOfferId.DeleteResponse.OfferNotDeleted) {
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync("Offer not deleted");
                } else {
                    context.Response.StatusCode = 200;
                    await context.Response.WriteAsync("Offer deleted");
                }
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

        app.MapPatch("/water/offer/{offer_id}", async context => {
            try {
                ValueTask<WaterOfferOfferId.PatchResponse> func = WaterOfferOfferId.Patch(
                    context.Request.Headers,
                    context.Request.Body,
                    context.Request.RouteValues,
                    _dbDataSource,
                    _dateTimeProvider,
                    _remoteManager
                );
                WaterOfferOfferId.PatchResponse r = await func;
                if(r == WaterOfferOfferId.PatchResponse.OfferNotFound) {
                    context.Response.StatusCode = 404;
                    await context.Response.WriteAsync("Offer not found");
                } else if(r == WaterOfferOfferId.PatchResponse.OfferNotPatched) {
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync("Offer not patched");
                } else {
                    context.Response.StatusCode = 200;
                    await context.Response.WriteAsync("Offer patched");
                }
            } catch(WaterOfferException e) {
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

        app.MapGet("/water/order", async context => {
            try {
                ValueTask<string> func = WaterOrder.Get(
                    context.Request.Headers,
                    _dbDataSource,
                    _dateTimeProvider,
                    _remoteManager
                );
                string data = await func;
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

        app.MapGet("/water/consumption", async context => {
            try {
                ValueTask<string> func = WaterConsumption.Get(
                    context.Request.Headers,
                    _dbDataSource,
                    _dateTimeProvider,
                    _remoteManager
                );
                string data = await func;
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
            try {
                ValueTask<string> func = Field.Patch(
                    context.Request.Headers,
                    context.Request.RouteValues,
                    context.Request.Body,
                    _dbDataSource,
                    _dateTimeProvider,
                    _remoteManager
                );
                string data = await func;
                context.Response.StatusCode = 201;
                await context.Response.WriteAsync(data);
            } catch(AuthorizationException e) {
                context.Response.StatusCode = 403;
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
                ValueTask<string> func = Fields.PostField(
                    context.Request.Headers,
                    context.Request.Body,
                    _dbDataSource,
                    _dateTimeProvider,
                    _remoteManager
                );
                string data = await func;
                context.Response.StatusCode = 200;
                await context.Response.WriteAsync(data);
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