

public class WebServer
{
    public void runServer()
    {
        var builder = WebApplication.CreateBuilder();
        var configuration = builder.Configuration;
        var app = builder.Build();
        app.Use(async (context, next) =>
        {
            //get jwt bearer from header and check if it's valid
            string? jwtBearer = context.Request.Headers["Authorization"];
            if (jwtBearer == null)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Missing Authorization header");
                return;
            }
            if (!jwtBearer.StartsWith("Bearer "))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Invalid Authorization header");
                return;
            }
            string jwt = jwtBearer.Substring(7);
            TokenOut instance = JwtTokenManager.jwtVerified(jwt);
            if (instance.success == false)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync(instance.error);
                return;
            }
            if (!instance.claims.TryGetValue("role", out _))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Missing role claim");
                return;
            }
            if (!instance.claims.TryGetValue("user_id", out _))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Missing user_id claim");
                return;
            }
            await next();
        });

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