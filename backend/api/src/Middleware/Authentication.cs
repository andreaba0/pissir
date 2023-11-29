using Microsoft.AspNetCore.Http;
using System.Web;
using Extension;

namespace Middleware;

public static class Authentication
{
    public static async Task JwtCheck(HttpContext context, Func<Task> next)
    {
        HttpResponseExtension httpResponseExtension = new HttpResponseExtension();
        //get jwt bearer from header and check if it's valid
        string? jwtBearer = context.Request.Headers["Authorization"];
        Console.WriteLine("init");
        if (jwtBearer == string.Empty || jwtBearer == null)
        {
            Console.WriteLine("Missing Authorization header");
            context.Response.StatusCode = 401;
            await httpResponseExtension.WriteAsync(context.Response, "Missing Authorization header");
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
    }
}