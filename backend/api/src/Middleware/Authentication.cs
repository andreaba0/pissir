using Microsoft.AspNetCore.Http;
using System.Web;
using Extension;
using System.Text.RegularExpressions;

namespace Middleware;

public static class Authentication
{
    public static bool IsAuthenticated(string authorizationHeader, out string jwt, out string error_message) {
        if(authorizationHeader == null || authorizationHeader == string.Empty) {
            jwt = string.Empty;
            error_message = "Missing Authorization header";
            return false;
        }
        Regex tokenRegex = new Regex(@"(?<scheme>[A-Za-z-]+)\s(?<token>[A-Za-z0-9-_\.]+)");
        if(!tokenRegex.IsMatch(authorizationHeader)) {
            jwt = string.Empty;
            error_message = "Invalid Authorization header";
            return false;
        }
        string scheme = tokenRegex.Match(authorizationHeader).Groups["scheme"].Value;
        if(scheme.ToLower() != "bearer") {
            jwt = string.Empty;
            error_message = $"Bearer scheme required but found {scheme}";
            return false;
        }
        string token = tokenRegex.Match(authorizationHeader).Groups["token"].Value;
        Regex jwtRegex = new Regex(@"^[A-Za-z0-9-_]+\.[A-Za-z0-9-_]+\.[A-Za-z0-9-_]+$");
        string jwtToken = jwtRegex.Match(token).Value;
        if(jwtToken == string.Empty) {
            jwt = string.Empty;
            error_message = "Invalid token format, expected Json Web Token";
            return false;
        }
        jwt = jwtToken;
        error_message = string.Empty;
        return true;
    }

    public static async Task JwtCheck(HttpContext context, Func<Task> next, IStaticWrapperHRE httpResponseExtension)
    {
        //get jwt bearer from header and check if it's valid
        string? jwtBearer = context.Request.Headers["Authorization"];
        if (jwtBearer == string.Empty || jwtBearer == null)
        {
            context.Response.StatusCode = 401;
            await httpResponseExtension.WriteAsync(context.Response, "Missing Authorization header");
            return;
        }
        if (!jwtBearer.StartsWith("Bearer "))
        {
            context.Response.StatusCode = 401;
            await httpResponseExtension.WriteAsync(context.Response, "Invalid Authorization header");
            return;
        }
        string jwt = jwtBearer.Substring(7);
        TokenOut instance = JwtTokenManager.jwtVerified(jwt);
        if (instance.success == false)
        {
            context.Response.StatusCode = 401;
            await httpResponseExtension.WriteAsync(context.Response, instance.error);
            return;
        }
        if (!instance.claims.TryGetValue("role", out _))
        {
            context.Response.StatusCode = 401;
            await httpResponseExtension.WriteAsync(context.Response, "Missing role claim");
            return;
        }
        if (!instance.claims.TryGetValue("user_id", out _))
        {
            context.Response.StatusCode = 401;
            await httpResponseExtension.WriteAsync(context.Response, "Missing user_id claim");
            return;
        }
        await next();
    }
    /*{
        HttpResponseExtension httpResponseExtension = new HttpResponseExtension();
        //get jwt bearer from header and check if it's valid
        string? jwtBearer = context.Request.Headers["Authorization"];
        Console.WriteLine("init");
        if (jwtBearer == string.Empty || jwtBearer == null)
        {
            Console.WriteLine("Missing Authorization header");
            context.Response.StatusCode = 401;
            //await httpResponseExtension.WriteAsync(context.Response, "Missing Authorization header");
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
    }*/
}