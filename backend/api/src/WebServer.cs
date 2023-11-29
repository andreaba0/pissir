using Middleware;

public class WebServer
{
    public void runServer()
    {
        var builder = WebApplication.CreateBuilder();
        var configuration = builder.Configuration;
        var app = builder.Build();
        app.Use(Authentication.JwtCheck);

        app.MapPost("/api/water/sell", async context =>
        {
            context.Response.StatusCode = 200;
            await context.Response.WriteAsync("OK");
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
        app.MapPost("/api/object/{type}", async context => {
            context.Response.StatusCode=200;
            await context.Response.WriteAsync("OK");
        });
        app.MapDelete("/api/object/{id}", async context => {
            context.Response.StatusCode=200;
            await context.Response.WriteAsync("OK");
        });
        app.MapPost("/api/company/field", async context => {
            context.Response.StatusCode=200;
            await context.Response.WriteAsync("OK");
        });
        app.MapPost("/api/water/limit/{company}", async context => {
            context.Response.StatusCode=200;
            await context.Response.WriteAsync("OK");
        });

        app.Run();
    }
}