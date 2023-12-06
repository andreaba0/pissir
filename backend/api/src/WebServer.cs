using Middleware;
using Extension;

public class WebServer
{
    private readonly DbDataSource _dbDataSource;
    public WebServer(
        DbDataSource dbDataSource
    )
    {
        this._dbDataSource = dbDataSource;
    }

    public void runServer()
    {
        var builder = WebApplication.CreateBuilder();
        var configuration = builder.Configuration;
        var app = builder.Build();
        //app.Use(async (context, next) => Authentication.JwtCheck(context, next, new HttpResponseExtension()));

        app.MapPost("/api/water/sell", async context =>
        {
            bool isAuthenticated = Authentication.IsAuthenticated(context.Request.Headers["Authorization"], out string jwt, out string message);
            if (!isAuthenticated)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync(message);
                return;
            }
            {
                bool isOk = JwtControl.GetClaims(jwt, out ClaimsPrincipal? principal, out string message);
                if (!isOk)
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync(message);
                    return;
                } else {
                    context.User = principal;
                }
            }
            {
                
            }
            context.Response.StatusCode = 200;
            await context.Response.WriteAsync("OK");
        });

        app.MapPost("/api/water/buy", async context =>
        {
            Tuple<uint, string> result = await new PostWaterBuy().HandleRoute(
                _dbDataSource,
                context.Request.Headers
            );
            context.Response.StatusCode = result.Item1;
            await context.Response.WriteAsync(result.Item2);
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
    }
}