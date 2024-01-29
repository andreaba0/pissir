using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Data.Common;
using System.Data;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Net;

using Module.KeyManager.Openid;

namespace Module.KeyManager;

public class LocalManager : Manager
{
    private readonly DbDataSource _dbDataSource;

    public enum OutCode
    {
        INVALID_ISSUER,
        UNKNOW_KEY,
        OK
    }
    public LocalManager(DbDataSource dbDataSource) : base()
    {
        _dbDataSource = dbDataSource;
    }

    protected override async Task UpdateRsaParameters()
    {
        try
        {
            await using var connection = await _dbDataSource.OpenConnectionAsync();
            var command = connection.CreateCommand();
            command.CommandText = @"
            SELECT id, d, dp, dq, exponent, inverse_q, modulus, p, q
            FROM rsa
            ORDER BY created_at DESC
            LIMIT 3";
            var parameters = new RSAKey[3];
            int index = 0;
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var id = reader.GetString(0);
                var d = (byte[])reader.GetValue(1);
                var dp = (byte[])reader.GetValue(2);
                var dq = (byte[])reader.GetValue(3);
                var exponent = (byte[])reader.GetValue(4);
                var inverseQ = (byte[])reader.GetValue(5);
                var modulus = (byte[])reader.GetValue(6);
                var p = (byte[])reader.GetValue(7);
                var q = (byte[])reader.GetValue(8);
                parameters[index++] = new RSAKey(
                    id,
                    new RSAParameters
                    {
                        D = d,
                        DP = dp,
                        DQ = dq,
                        Exponent = exponent,
                        InverseQ = inverseQ,
                        Modulus = modulus,
                        P = p,
                        Q = q
                    }
                );
            }
            if (index != 3)
            {
                throw new LocalManagerException(LocalManagerException.ErrorCode.FAILED_TO_UPDATE_RSA_PARAMETERS, "Failed to update RSA parameters");
            }
            await base.SwitchArray(parameters);
        }
        catch (Exception ex)
        {
            base.SwitchArray(new RSAKey[0]);
            throw ex;
        }
    }

    public RSAKey GetSignKey()
    {
        RSAKey[] parameters = base.GetRsaParameters();
        return parameters[1];
    }
}

public class QueryKeyService
{
    private readonly DbDataSource _dbDataSource;
    private readonly IRemoteJwksHub _jwksHub;

    public QueryKeyService(
        DbDataSource dbDataSource,
        IRemoteJwksHub jwksHub
    )
    {
        _dbDataSource = dbDataSource;
        _jwksHub = jwksHub;
    }

    public async Task<List<ProviderInfo>> QueryJwksEndpointAsync()
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

    public async Task<int> RunAsync(CancellationToken ct)
    {
        List<ProviderInfo>? providers = default(List<ProviderInfo>);
        while (true)
        {
            Console.WriteLine("Querying jwks endpoint...");
            try
            {
                providers = await QueryJwksEndpointAsync();
                break;
            }
            catch (Exception)
            {
                await Task.Delay(1000);
            }
        }

        var tasks = new List<Task>();
        await _jwksHub.SetupAsync(providers);
        tasks.Add(_jwksHub.RunAsync(ct));
        await Task.WhenAll(tasks);
        return 0;
    }
}

public class LocalManagerException : Exception
{
    public enum ErrorCode
    {
        INVALID_ISSUER,
        UNKNOW_KEY,
        FAILED_TO_UPDATE_RSA_PARAMETERS
    }
    public ErrorCode Code { get; }
    public LocalManagerException(ErrorCode code) : base()
    {
        this.Code = code;
    }
    public LocalManagerException(ErrorCode code, String message) : base(message)
    {
        this.Code = code;
    }
    public LocalManagerException(ErrorCode code, String message, Exception innerException) : base(message, innerException)
    {
        this.Code = code;
    }
}