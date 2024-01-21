using System.Data.Common;

using Middleware;
using Extension;
using Types;
using System.Security.Claims;
using Interface.Utility;
using Module.JsonWebToken;
using Interface.Module.JsonWebToken;
using System.Security.Cryptography;

public class WebServer
{
    private readonly DbDataSource _dbDataSource;
    private readonly JwtControl _jwtControl;
    private readonly IClockCustom _clock;
    private IKeyService _keyManager;
    public WebServer(
        DbDataSource dbDataSource,
        JwtControl jwtControl,
        IClockCustom clock,
        IKeyService keyManager
    )
    {
        this._dbDataSource = dbDataSource;
        this._jwtControl = jwtControl;
        this._clock = clock;
        this._keyManager = keyManager;
    }

    public void runServer()
    {
        Console.WriteLine("Server started");
        var builder = WebApplication.CreateBuilder();
        var configuration = builder.Configuration;
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

        app.MapPost("/api/water/buy", async context =>
        {
            context.Response.StatusCode = 200;
            await context.Response.WriteAsync("OK");
        });
        app.MapPost("/api/water/offer", async context =>
        {

            context.Response.StatusCode = 200;
            await context.Response.WriteAsync("OK");
        });
        app.MapPost("/api/company/field", async context =>
        {
            context.Response.StatusCode = 200;
            await context.Response.WriteAsync("OK");
        });
        app.MapDelete("/api/company/{field}", async context =>
        {
            context.Response.StatusCode = 200;
            await context.Response.WriteAsync("OK");
        });
        app.MapGet("/api/analytics/{field}/{type}", async context =>
        {
            context.Response.StatusCode = 200;
            await context.Response.WriteAsync("OK");
        });
        app.MapPost("/api/object/{type}", async context =>
        {
            context.Response.StatusCode = 200;
            await context.Response.WriteAsync("OK");
        });
        app.MapDelete("/api/object/{id}", async context =>
        {
            context.Response.StatusCode = 200;
            await context.Response.WriteAsync("OK");
        });
        app.MapPost("/api/company/field", async context =>
        {
            context.Response.StatusCode = 200;
            await context.Response.WriteAsync("OK");
        });
        app.MapPost("/api/water/limit/{company}", async context =>
        {
            context.Response.StatusCode = 200;
            await context.Response.WriteAsync("OK");
        });

        app.Run();
        Console.WriteLine("Server stopped");
    }
}