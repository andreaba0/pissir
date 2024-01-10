using System.Data.Common;
using System.Data;
using System.Threading;
using System.Security.Cryptography;
using Npgsql;
using NpgsqlTypes;

using Utility;

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Collections.Concurrent;

using MQTTConcurrent;
using MQTTConcurrent.Message;

using Module.KeyManager;

namespace Module.WebServer;

internal class RSAParameterArray {
    public RSAParameters[] parameters { get; set; }
}
public class WebServer
{
    private readonly DbDataSource _dbDataSource;
    private Manager _keyManager;
    private RemoteManager _remoteManager;
    private int _port;

    public WebServer(DbDataSource dbDataSource, int port)
    {
        _dbDataSource = dbDataSource;
        _port = port;
        _keyManager = new LocalManager(
            _dbDataSource
        );
        _remoteManager = new RemoteManager(
            "https://www.googleapis.com/oauth2/v3/certs",
            new Fetch()
        );
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

        app.MapGet("/oauth/google/jwks", async context => {
            RSAArrayElement[] keys = _remoteManager.GetKeys();
            //return the keys as json
            //KeyArray keyArray = new KeyArray(keys);
            var json = JsonSerializer.Serialize(keys);
            context.Response.ContentType = "application/json";
            context.Response.Headers.Add("Cache-Control", "max-age=3600");
            await context.Response.WriteAsync(json);
        });

        app.MapPost("/api/key", async context =>
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

        app.MapGet("/.well-known/oauth/openid/jwks", async context => {
            KeyJson[] keys = new KeyJson[3];
            RSAArrayElement[]? _rsaParameters;
            bool isOk = _keyManager.GetRsaParameters(out _rsaParameters);
            if(!isOk) {
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("Internal Server Error");
                return;
            }
            for(int i=0; i<3; i++) {
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