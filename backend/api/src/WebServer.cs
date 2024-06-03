using System.Data.Common;

using Middleware;
using Extension;
using Types;
using System.Security.Claims;
using Interface.Utility;
using Module.JsonWebToken;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Net;

using Module.KeyManager;

public class WebServer
{
    private readonly DbDataSource _dbDataSource;
    private readonly JwtControl _jwtControl;
    private readonly IClockCustom _clock;
    private readonly HttpClient _httpClient;
    private RemoteManager _keyManager;
    public WebServer(
        DbDataSource dbDataSource,
        JwtControl jwtControl,
        IClockCustom clock,
        HttpClient httpClient,
        RemoteManager keyManager
    )
    {
        this._dbDataSource = dbDataSource;
        this._jwtControl = jwtControl;
        this._clock = clock;
        this._keyManager = keyManager;
        this._httpClient = httpClient;
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        Console.WriteLine("Server started");
        var builder = WebApplication.CreateBuilder();
        var configuration = builder.Configuration;
        //change Kestrel port
        builder.WebHost.UseKestrel(options =>
        {
            options.Listen(IPAddress.Any, 5000);
        });
        var app = builder.Build();

        app.MapPost("/api/water/sell", async context =>
        {
            context.Response.StatusCode = 200;
            await context.Response.WriteAsync("OK");
        });

        app.MapGet("/api/ping", async context =>
        {
            context.Response.StatusCode = 200;
            await context.Response.WriteAsync("OK");
        });

        app.MapGet("/certs", async context => {
            bool isOk = _keyManager.GetRsaParameters(out RSAKey[] keys);
            if(!isOk) {
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("Internal Server Error");
                return;
            }
            context.Response.StatusCode = 200;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(keys));
        });

        app.MapGet("/api/key/{kid}", async context =>
        {
            string kid = context.Request.RouteValues["kid"].ToString();
            RSAParameters? key = _keyManager.GetKey(kid);
            if (key is null)
            {
                context.Response.StatusCode = 404;
                await context.Response.WriteAsync("Not Found");
                return;
            } else {
                context.Response.StatusCode = 200;
                await context.Response.WriteAsync("OK");
                return;
            }
        });

        app.MapPost("/water/buy", async context =>
        {
            context.Response.StatusCode = 200;
            await context.Response.WriteAsync("OK");
        });
        app.MapPost("/water/offer", async context =>
        {
            context.Response.StatusCode = 200;
            await context.Response.WriteAsync("OK");
        });
        app.MapPost("/api/company/field", async context =>
        {
            context.Response.StatusCode = 200;
            await context.Response.WriteAsync("OK");
        });
        app.MapDelete("/{cid}/{field}", async context =>
        {
            context.Response.StatusCode = 200;
            await context.Response.WriteAsync("OK");
        });
        app.MapGet("/analytics/{field}/{type}", async context =>
        {
            context.Response.StatusCode = 200;
            await context.Response.WriteAsync("OK");
        });
        app.MapPost("/object/{type}", async context =>
        {
            context.Response.StatusCode = 200;
            await context.Response.WriteAsync("OK");
        });
        app.MapDelete("/object/{id}", async context =>
        {
            context.Response.StatusCode = 200;
            await context.Response.WriteAsync("OK");
        });
        app.MapPost("/company/field", async context =>
        {
            context.Response.StatusCode = 200;
            await context.Response.WriteAsync("OK");
        });
        app.MapPost("/water/limit/{company}", async context =>
        {
            context.Response.StatusCode = 200;
            await context.Response.WriteAsync("OK");
        });

        
        Task[] tasks = new Task[] {
            _keyManager.RunAsync(ct),
            app.RunAsync()
        };
        await Task.WhenAll(tasks);

        Console.WriteLine("Server stopped");
    }
}