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

public interface ILocalManager
{
    RSAKey GetSignKey();
}

public class LocalManager
{
    private readonly DbDataSource _dbDataSource;
    private RSAKey[] _rsaParameters = new RSAKey[0];
    private object _lock = new object();

    public class RSAKey {
        public string kid { get; set; }
        public RSAParameters parameters { get; set; }
        public RSAKey(
            string kid,
            RSAParameters parameters
        ) {
            this.kid = kid;
            this.parameters = parameters;
        }
    }

    public LocalManager(DbDataSource dbDataSource)
    {
        _dbDataSource = dbDataSource;
    }

    private async Task UpdateRsaParameters()
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
                var id = reader.GetGuid(0).ToString();
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
            lock (_lock)
            {
                _rsaParameters = parameters;
            }
        }
        catch (Exception ex)
        {
            lock (_lock)
            {
                _rsaParameters = new RSAKey[0];
            }
            throw ex;
        }
    }

    public async Task<int> RunAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                Console.WriteLine("Querying RSA keys from database...");
                await UpdateRsaParameters();
                Console.WriteLine("Query RSA keys from database: SUCCESS");
                await Task.Delay(1000*60*5);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await Task.Delay(1000);
            }
        }
        return 0;
    }

    public RSAKey GetSignKey()
    {
        RSAKey signKey;
        lock (_lock)
        {
            signKey = new RSAKey(
                _rsaParameters[1].kid,
                new RSAParameters
                {
                    D = _rsaParameters[1].parameters.D,
                    DP = _rsaParameters[1].parameters.DP,
                    DQ = _rsaParameters[1].parameters.DQ,
                    Exponent = _rsaParameters[1].parameters.Exponent,
                    InverseQ = _rsaParameters[1].parameters.InverseQ,
                    Modulus = _rsaParameters[1].parameters.Modulus,
                    P = _rsaParameters[1].parameters.P,
                    Q = _rsaParameters[1].parameters.Q
                }
            );
        }
        return signKey;
    }

    public string GetRsaParameters()
    {
        lock (_lock)
        {
            return JsonSerializer.Serialize(_rsaParameters);
        }
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

    public async Task QueryJwksEndpointAsync()
    {
        await using var connection = await _dbDataSource.OpenConnectionAsync();
        using var command = connection.CreateCommand();
        command.CommandText = @"
                select 
                    provider_name, 
                    configuration_uri, 
                    is_active,
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
        while (await reader.ReadAsync())
        {
            var name = reader.GetString(0);
            var configurationUri = reader.GetString(1);
            var is_active = reader.GetBoolean(2);
            var audienceList = reader.GetString(3);
            if (!is_active)
            {
                _jwksHub.CancelProvider(name);
            }
            var audience = JsonSerializer.Deserialize<string[]>(audienceList);
            ProviderInfo provider = new ProviderInfo(name, configurationUri, audience);
            _jwksHub.AddProvider(provider);
        }
    }

    public async Task<int> RunAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            Console.WriteLine("Querying jwks endpoint from database...");
            try
            {
                await QueryJwksEndpointAsync();
                break;
            }
            catch (DbException e)
            {
                Console.WriteLine(e.Message);
                await Task.Delay(1000);
            }
            catch(Exception e){
                Console.WriteLine(e);
                await Task.Delay(1000);
            }
        }
        return 0;
    }
}

public class LocalManagerException : Exception
{
    public enum ErrorCode
    {
        GENERIC_ERROR = 0,
        INVALID_ISSUER = 1,
        UNKNOW_KEY = 2,
        FAILED_TO_UPDATE_RSA_PARAMETERS = 3,
        CURRENTLY_UNAVAILABLE = 4
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