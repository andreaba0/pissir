using System.Data.Common;
using System.Data;
using System.Threading;
using System.Security.Cryptography;
using Npgsql;
using NpgsqlTypes;

using Utility;
using Routes;

using Module.KeyManager.Openid;
using Module.Middleware;

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Collections.Concurrent;

using Module.KeyManager;

namespace Module.WebServer;

public class WebServer
{
    private readonly DbDataSource _dbDataSource;
    private LocalManager _keyManager;
    private readonly IRemoteJwksHub _remoteManager;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly string _localIssuer;

    public WebServer(
        DbDataSource dbDataSource, 
        IRemoteJwksHub remoteManager,
        LocalManager keyManager,
        IDateTimeProvider dateTimeProvider,
        string localIssuer)
    {
        _dbDataSource = dbDataSource;
        _remoteManager = remoteManager;
        _keyManager = keyManager;
        _dateTimeProvider = dateTimeProvider;
        _localIssuer = localIssuer;
    }

    public async Task<int> RunAsync(CancellationToken cancellationToken = default)
    {
        //create a web server with WebApplication builder that listen on port wit manuel route handling
        var builder = WebApplication.CreateBuilder();
        var configuration = builder.Configuration;
        var app = builder.Build();

        app.MapGet("/ping", async context =>
        {
            await context.Response.WriteAsync("pong");
        });

        app.MapPost("/openid/key", async context =>
        {
            try
            {
                KeyRotator.PostMethod_Rotate(
                    context.Request.Headers,
                    _dbDataSource
                ).Wait();
                context.Response.StatusCode = 200;
                await context.Response.WriteAsync("OK");
            }
            catch (AuthenticationException e)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync((e.Code != default(AuthenticationException.ErrorCode)) ? e.Message : "");
            }
            catch (DbException e)
            {
                Console.WriteLine(e);
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("Internal Server Error");
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e);
                Console.WriteLine(e);
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("Internal Server Error");
            }
        });

        app.MapGet("/profile", async context =>
        {
            try
            {
                Profile profile = Profile.GetMethod(
                    context.Request.Headers,
                    _dbDataSource,
                    _dateTimeProvider,
                    _remoteManager
                );
                var json = JsonSerializer.Serialize(profile);
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(json);
            }
            catch (AuthenticationException e)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync((e.Code != default(AuthenticationException.ErrorCode)) ? e.Message : "");
            }
            catch(AuthorizationException e) {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync((e.Code != default(AuthorizationException.ErrorCode)) ? e.Message : "");
            }
            catch (DbException e)
            {
                Console.WriteLine(e);
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("Server error");
            }
            catch (ProfileException e)
            {
                if (e.Code == ProfileException.ErrorCode.USER_NOT_FOUND)
                {
                    context.Response.StatusCode = 404;
                    await context.Response.WriteAsync("Not Found");
                }
                else
                {
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsync("Server error");
                }
            }
            catch (Exception)
            {
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("Server error");
            }
        });

        app.MapGet("/.well-known/jwks.json", async context =>
        {
            try
            {
                string _rsaParameters = _keyManager.GetRsaParameters();
                context.Response.ContentType = "application/json";
                context.Response.Headers.Append("Cache-Control", "max-age=3600");
                await context.Response.WriteAsync(_rsaParameters);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("Internal Server Error");
            }
        });

        app.MapPost("/service/apply", async context =>
        {
            try
            {
                Application.PostMethod_Apply(
                    context.Request.Headers,
                    context.Request.Body,
                    _dbDataSource,
                    _dateTimeProvider,
                    _remoteManager
                ).Wait();
                context.Response.StatusCode = 201;
                await context.Response.WriteAsync("Created");
            }
            catch (AuthenticationException e)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync((e.Code != default(AuthenticationException.ErrorCode)) ? e.Message : "");
            }
            catch (DbException)
            {
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("Server error");
            }
            catch (Routes.ApplicationException e)
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync((e.Code != default(Routes.ApplicationException.ErrorCode)) ? e.Message : "");
            }
            catch (Exception)
            {
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("Server error");
            }
        });

        app.MapGet("/service/application", async context =>
        {
            try
            {
                Task<string> applicationTask = Application.GetMethod_Applications(
                    context.Request.Headers,
                    context.Request.Query,
                    _dbDataSource,
                    _dateTimeProvider,
                    _remoteManager
                );
                context.Response.StatusCode = 200;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(applicationTask.Result);
            }
            catch (AuthenticationException e)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync((e.Code != default(AuthenticationException.ErrorCode)) ? e.Message : "");
            }
            catch (DbException)
            {
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("Server error");
            }
            catch (Routes.ApplicationException e)
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync((e.Code != default(Routes.ApplicationException.ErrorCode)) ? e.Message : "");
            }
            catch (Exception)
            {
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("Server error");
            }
        });

        app.MapGet("/service/my_application", async context =>
        {
            try
            {
                Task<string> applicationTask = Application.GetMethod_MyApplication(
                    context.Request.Headers,
                    _dbDataSource,
                    _dateTimeProvider,
                    _remoteManager
                );
                context.Response.StatusCode = 200;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(applicationTask.Result);
            }
            catch (AuthenticationException e)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync((e.Code != default(AuthenticationException.ErrorCode)) ? e.Message : "");
            }
            catch (DbException)
            {
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("Server error");
            }
            catch (Routes.ApplicationException e)
            {
                if(e.Code == Routes.ApplicationException.ErrorCode.APPLICATION_NOT_FOUND)
                {
                    context.Response.StatusCode = 404;
                    await context.Response.WriteAsync("Not Found");
                }
                else
                {
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync((e.Code != default(Routes.ApplicationException.ErrorCode)) ? e.Message : "");
                }
            }
            catch (Exception)
            {
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("Server error");
            }
        });

        app.MapPost("/service/application/{id}/{action}", async context =>
        {
            try
            {
                Application.PostMethod_ManageApplication(
                    context.Request.Headers,
                    context.Request.RouteValues,
                    _dbDataSource,
                    _dateTimeProvider,
                    _remoteManager
                ).Wait();
                context.Response.StatusCode = 200;
                await context.Response.WriteAsync("OK");
            }
            catch (AuthenticationException e)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync((e.Code != default(AuthenticationException.ErrorCode)) ? e.Message : "");
            }
            catch(AuthorizationException e) {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync((e.Code != default(AuthorizationException.ErrorCode)) ? e.Message : "");
            }
            catch (DbException)
            {
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("Server error");
            }
            catch (Routes.ApplicationException e)
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync((e.Code != default(Routes.ApplicationException.ErrorCode)) ? e.Message : "");
            }
            catch (Exception)
            {
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("Server error");
            }
        });

        app.MapGet("/apiaccess", async context => {
            try
            {
                Task<List<ApiAccess.AccessRequestRow>> apiAccessTask = ApiAccess.GetMethod_ACLRequest(
                    context.Request.Headers,
                    context.Request.Query,
                    _dbDataSource,
                    _dateTimeProvider,
                    _remoteManager
                );
                context.Response.StatusCode = 200;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(await apiAccessTask));
            }
            catch (AuthenticationException e)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync((e.Code != default(AuthenticationException.ErrorCode)) ? e.Message : "");
            }
            catch (AuthorizationException e) {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync((e.Code != default(AuthorizationException.ErrorCode)) ? e.Message : "");
            }
            catch (DbException e)
            {
                Console.WriteLine(e);
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("Server error");
            }
            catch (Routes.ApiAccessException e)
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync((e.Code != default(Routes.ApiAccessException.ErrorCode)) ? e.Message : "");
            }
            catch (Exception)
            {
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("Server error");
            }
        });

        app.MapPost("/apiaccess", async context =>
        {
            try {
                ApiAccess.PostMethod_ACLRequest(
                    context.Request.Headers,
                    context.Request.Body,
                    _dbDataSource,
                    _dateTimeProvider,
                    _remoteManager
                ).Wait();
                context.Response.StatusCode = 201;
                await context.Response.WriteAsync("Created");
            }
            catch (AuthenticationException e)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync((e.Code != default(AuthenticationException.ErrorCode)) ? e.Message : "");
            }
            catch (AuthorizationException e) {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync((e.Code != default(AuthorizationException.ErrorCode)) ? e.Message : "");
            }
            catch (DbException)
            {
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("Server error");
            }
            catch (Routes.ApiAccessException e)
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync((e.Code != default(Routes.ApiAccessException.ErrorCode)) ? e.Message : "");
            }
            catch (Exception)
            {
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("Server error");
            }
        });

        app.MapPost("/apiaccess/{id}/{action}", async context =>
        {
            try {
                ApiAccess.PostMethod_ACLAction(
                    context.Request.Headers,
                    context.Request.RouteValues,
                    _dbDataSource,
                    _dateTimeProvider,
                    _remoteManager
                ).Wait();
                context.Response.StatusCode = 200;
                await context.Response.WriteAsync("OK");
            }
            catch (AuthenticationException e)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync((e.Code != default(AuthenticationException.ErrorCode)) ? e.Message : "");
            }
            catch (AuthorizationException e) {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync((e.Code != default(AuthorizationException.ErrorCode)) ? e.Message : "");
            }
            catch (DbException)
            {
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("Server error");
            }
            catch (Routes.ApiAccessException e)
            {
                Console.WriteLine(e);
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync((e.Code != default(Routes.ApiAccessException.ErrorCode)) ? e.Message : "");
            }
            catch (Exception)
            {
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("Server error");
            }
        });

        app.MapPost("/token", async context => {
            try
            {
                string token = ApiAccess.PostMethod_Token(
                    context.Request.Headers,
                    _dbDataSource,
                    _dateTimeProvider,
                    _remoteManager,
                    _keyManager,
                    _localIssuer
                ).Result;
                context.Response.StatusCode = 200;
                context.Response.ContentType = "plain/text";
                await context.Response.WriteAsync(token);
            }
            catch (AuthenticationException e)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync((e.Code != default(AuthenticationException.ErrorCode)) ? e.Message : "");
            }
            catch (AuthorizationException e) {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync((e.Code != default(AuthorizationException.ErrorCode)) ? e.Message : "");
            }
            catch (DbException)
            {
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("Server error");
            }
            catch (Exception)
            {
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("Server error");
            }
        });

        app.MapGet("/company", async context =>
        {
            try
            {
                Task<string> companyTask = Company.GetMethod_CompanyInfo(
                    context.Request.Headers,
                    _dbDataSource,
                    _dateTimeProvider,
                    _remoteManager
                );
                context.Response.StatusCode = 200;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(companyTask.Result);
            }
            catch (AuthenticationException e)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync((e.Code != default(AuthenticationException.ErrorCode)) ? e.Message : "");
            }
            catch (DbException e)
            {
                Console.WriteLine(e);
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("Server error");
            }
            catch (Routes.CompanyException e)
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync((e.Code != default(Routes.CompanyException.ErrorCode)) ? e.Message : "");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("Server error");
            }
        });

        app.MapPatch("/company", async context => {
            try
            {
                Company.PatchMethod_CompanyInfo(
                    context.Request.Headers,
                    context.Request.Body,
                    _dbDataSource,
                    _dateTimeProvider,
                    _remoteManager
                ).Wait();
                context.Response.StatusCode = 200;
                await context.Response.WriteAsync("OK");
            }
            catch (AuthenticationException e)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync((e.Code != default(AuthenticationException.ErrorCode)) ? e.Message : "");
            }
            catch (DbException e)
            {
                Console.WriteLine(e);
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("Server error");
            }
            catch (Routes.CompanyException e)
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync((e.Code != default(Routes.CompanyException.ErrorCode)) ? e.Message : "");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("Server error");
            }
        });

        app.MapGet("/company/{id}", async context =>
        {
            context.Response.StatusCode = 200;
            await context.Response.WriteAsync("OK");
        });

        Task[] tasks = new Task[] {
            _keyManager.RunAsync(cancellationToken),
            app.RunAsync(cancellationToken)
        };
        await Task.WhenAll(tasks);
        Console.WriteLine("WebServer stopped");

        return 0;
    }
}