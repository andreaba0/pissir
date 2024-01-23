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

using MQTTConcurrent;
using MQTTConcurrent.Message;

using Module.KeyManager;

namespace Module.WebServer;

internal class RSAParameterArray
{
    public RSAParameters[] parameters { get; set; }
}
public class WebServer
{
    private readonly DbDataSource _dbDataSource;
    private Manager _keyManager;
    private readonly IRemoteJwksHub _remoteManager;

    public WebServer(DbDataSource dbDataSource, IRemoteJwksHub remoteManager)
    {
        _dbDataSource = dbDataSource;
        _remoteManager = remoteManager;
        _keyManager = new LocalManager(
            _dbDataSource
        );
    }

    public async Task<List<ProviderInfo>> QueryJwksEndpointAsync()
    {
        //query list of jwks endpoint from database
        try
        {
            await using var connection = await _dbDataSource.OpenConnectionAsync();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                select 
                    provider_name, 
                    configuration_uri, 
                    to_json(
                        array_agg(
                            audience
                        )
                    ) as audicence_list 
                from 
                    registered_provider as rp, 
                    allowed_audience as aa 
                where 
                    aa.registered_provider=rp.provider_name 
                group by 
                    rp.provider_name
            ";
            var reader = await command.ExecuteReaderAsync();
            List<ProviderInfo> providers = new List<ProviderInfo>();
            while (await reader.ReadAsync())
            {
                var name = reader.GetString(0);
                var configurationUri = reader.GetString(1);
                var audienceList = reader.GetString(2);
                var audience = JsonSerializer.Deserialize<string[]>(audienceList);
                providers.Add(new ProviderInfo(name, configurationUri, audience));
            }
            return providers;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return new List<ProviderInfo>();
        }
    }

    public async Task<int> RunAsync(CancellationToken cancellationToken = default)
    {

        List<ProviderInfo> providers = await QueryJwksEndpointAsync();
        await _remoteManager.SetupAsync(providers);

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
            //create a new rsa key and insert the key parameters to database table rsa
            var rsa = RSA.Create();
            var parameters = rsa.ExportParameters(true);
            var keyId = Guid.NewGuid().ToString();

            //insert the key using Npgsql as DbDataSource without calling undeclared methods
            await using var connection = await _dbDataSource.OpenConnectionAsync();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO rsa (id, d, dp, dq, exponent, inverse_q, modulus, p, q, created_at)
                VALUES ($1, $2, $3, $4, $5, $6, $7, $8, $9, $10) RETURNING id
            ";

            DbParameter[] dbParameters = new DbParameter[] {
                CreateParameter(connection, DbType.String, keyId),
                CreateParameter(connection, DbType.Binary, parameters.D),
                CreateParameter(connection, DbType.Binary, parameters.DP),
                CreateParameter(connection, DbType.Binary, parameters.DQ),
                CreateParameter(connection, DbType.Binary, parameters.Exponent),
                CreateParameter(connection, DbType.Binary, parameters.InverseQ),
                CreateParameter(connection, DbType.Binary, parameters.Modulus),
                CreateParameter(connection, DbType.Binary, parameters.P),
                CreateParameter(connection, DbType.Binary, parameters.Q),
                CreateParameter(connection, DbType.DateTime, DateTime.UtcNow)
            };
            command.Parameters.AddRange(dbParameters);

            Console.WriteLine(connection.State);

            try
            {
                var value = await command.ExecuteScalarAsync();
                //check if the key was inserted
                if (value == null)
                {
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsync("Internal Server Error");
                    return;
                }
                context.Response.StatusCode = 201;
                await context.Response.WriteAsync("OK");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
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
                    new DateTimeProvider(),
                    _remoteManager
                );
                var json = JsonSerializer.Serialize(profile);
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(json);
            }
            catch (AuthenticationException e)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized");
            }
            catch (DbException e)
            {
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("Server error");
            }
            catch (Exception e)
            {
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("Server error");
            }
        });

        app.MapGet("/.well-known/jwks.json", async context =>
        {
            KeyJson[] keys = new KeyJson[3];
            try
            {
                RSAKey[] _rsaParameters = _keyManager.GetRsaParameters();
                for (int i = 0; i < 3; i++)
                {
                    keys[i] = new KeyJson(
                        _rsaParameters[i].Id,
                        _rsaParameters[i].Parameters
                    );
                }
                KeyArray keyArray = new KeyArray(keys);
                var json = JsonSerializer.Serialize(keyArray);
                context.Response.ContentType = "application/json";
                context.Response.Headers.Add("Cache-Control", "max-age=3600");
                await context.Response.WriteAsync(json);
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
            context.Response.StatusCode = 200;
            await context.Response.WriteAsync("OK");
        });

        app.MapGet("/service/application", async context =>
        {
            context.Response.StatusCode = 200;
            await context.Response.WriteAsync("OK");
        });

        app.MapPost("/service/application/{id}/accept", async context =>
        {
            context.Response.StatusCode = 200;
            await context.Response.WriteAsync("OK");
        });

        app.MapPost("/service/application/{id}/reject", async context =>
        {
            context.Response.StatusCode = 200;
            await context.Response.WriteAsync("OK");
        });

        app.MapPost("/service/application/{id}/cancel", async context =>
        {
            context.Response.StatusCode = 200;
            await context.Response.WriteAsync("OK");
        });

        app.MapPost("/apiaccess", async context =>
        {
            context.Response.StatusCode = 200;
            await context.Response.WriteAsync("OK");
        });

        app.MapGet("/apiaccess", async context =>
        {
            context.Response.StatusCode = 200;
            await context.Response.WriteAsync("OK");
        });

        app.MapGet("/apiaccess/{id}", async context =>
        {
            context.Response.StatusCode = 200;
            await context.Response.WriteAsync("OK");
        });

        app.MapPost("/apiaccess/{id}/accept", async context =>
        {
            context.Response.StatusCode = 200;
            await context.Response.WriteAsync("OK");
        });

        app.MapPost("/apiaccess/{id}/reject", async context =>
        {
            context.Response.StatusCode = 200;
            await context.Response.WriteAsync("OK");
        });

        app.MapGet("/company/{id}", async context =>
        {
            context.Response.StatusCode = 200;
            await context.Response.WriteAsync("OK");
        });

        Task[] tasks = new Task[] {
            _keyManager.RunAsync(cancellationToken),
            app.RunAsync(cancellationToken),
            _remoteManager.RunAsync(cancellationToken)
        };
        await Task.WhenAll(tasks);

        return 0;
    }

    DbParameter CreateParameter(DbConnection connection, DbType type, object? value)
    {
        DbParameter parameter = DbProviderFactories.GetFactory(connection).CreateParameter();
        parameter.DbType = type;
        parameter.Value = value;
        return parameter;
    }
}