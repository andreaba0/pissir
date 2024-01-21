using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Data.Common;
using System.Data;
using System.Text.Json;
using System.Text.Json.Serialization;
using Types;
using Utility;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Net;

using Module.KeyManager.Openid;

namespace Module.KeyManager;

public class RSAKey : ICloneable
{
    public string Id { get; set; }
    public RSAParameters Parameters { get; set; }
    public RSAKey(string id, RSAParameters parameters)
    {
        this.Id = id;
        this.Parameters = parameters;
    }

    public object Clone() {
        return new RSAKey(Id, new RSAParameters {
            D = Parameters.D,
            DP = Parameters.DP,
            DQ = Parameters.DQ,
            Exponent = Parameters.Exponent,
            InverseQ = Parameters.InverseQ,
            Modulus = Parameters.Modulus,
            P = Parameters.P,
            Q = Parameters.Q
        });
    }
}

public class KeyArray
{
    public KeyJson[] keys { get; set; }
    public KeyArray(KeyJson[] keys)
    {
        this.keys = keys;
    }
    public static string Serialize(KeyArray keyArray)
    {
        return JsonSerializer.Serialize(keyArray);
    }
}

public class KeyJson
{
    public string alg { get; set; }
    public string kid { get; set; }
    public string n { get; set; }
    public string e { get; set; }
    public string use { get; set; }
    public string kty { get; set; }
    public KeyJson(string kid, RSAParameters parameters)
    {
        alg = "RS256";
        this.kid = kid;
        n = Convert.ToBase64String(parameters.Modulus);
        e = Convert.ToBase64String(parameters.Exponent);
        use = "sig";
        kty = "RSA";
    }

    public KeyJson(string kid, string n, string e, string use, string kty, string alg)
    {
        this.alg = alg;
        this.kid = kid;
        this.n = n;
        this.e = e;
        this.use = use;
        this.kty = kty;
    }

    public KeyJson() {

    }
    public static string Serialize(KeyJson keyJson)
    {
        return JsonSerializer.Serialize(keyJson);
    }
}

public abstract class Manager {
    private RSAKey?[] _rsaParameters;
    private object _lock = new object();

    public Manager()
    {
        _rsaParameters = new RSAKey[3];
    }

    public async Task<int> RunAsync(CancellationToken tk)
    {
        while (!tk.IsCancellationRequested)
        {
            while (!await UpdateRsaParameters())
            {
                await Task.Delay(1000, tk);
            }
            await Task.Delay(60 * 5 * 1000, tk);
        }
        return 0;
    }

    internal abstract Task<bool> UpdateRsaParameters();

    public bool GetRsaParameters(out RSAKey[]? parameters)
    {
        lock(_lock) {
            if(_rsaParameters == null) {
                parameters = null;
                return false;
            }
            RSAKey?[] _copy = new RSAKey[this._rsaParameters.Length];
            for(int i=0;i<_copy.Length;i++) {
                _copy[i] = (RSAKey?)_rsaParameters[i]?.Clone();
            }
            parameters = _copy as RSAKey[];
            return true;
        }
    }

    protected Task SwitchArray(RSAKey[]? parameters) {
        lock(_lock) {
            Interlocked.Exchange(ref this._rsaParameters, parameters);
        }
        return Task.CompletedTask;
    }
}

public class LocalManager : Manager
{
    private readonly DbDataSource _dbDataSource;
    public LocalManager(DbDataSource dbDataSource) : base()
    {
        _dbDataSource = dbDataSource;
    }

    internal override async Task<bool> UpdateRsaParameters() {
        try
        {
            await using var connection = await _dbDataSource.OpenConnectionAsync();
            using var command = connection.CreateCommand();
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
                base.SwitchArray(null);
                return false;
            }
            base.SwitchArray(parameters);
            return true;
        }
        catch (Exception ex)
        {
            base.SwitchArray(null);
            return false;
        }
    }

    public RSAKey? GetSignKey() {
        RSAKey[]? parameters;
        bool isOk = base.GetRsaParameters(out parameters);
        if(!isOk) {
            return null;
        }
        return parameters[1];
    }
}

public class RemoteManager : Manager {
    private readonly string _uri;
    private readonly IFetch _fetch;
    private static readonly char[] padding = { '=' };
    public RemoteManager(string uri, IFetch fetch) : base()
    {
        _uri = uri;
        _fetch = fetch;
    }

    internal override async Task<bool> UpdateRsaParameters() {
        try
        {
            IFetchResponseCustom response = await _fetch.Get(_uri);
            if(response == null) return false;
            if(response.StatusCode!=HttpStatusCode.OK) return false;
            KeyArray? json = JsonSerializer.Deserialize<KeyArray>(response.Content);
            if(json == null) return false;
            RSAKey?[] _rsaParameters = new RSAKey[json.keys.Length];
            for(int i=0; i<json.keys.Length; i++) {
                string kid = json.keys[i].kid;
                string nBase64 = json.keys[i].n.Replace('-', '+').Replace('_', '/');
                if(nBase64.Length % 4 > 0) {
                    nBase64 = nBase64.PadRight(nBase64.Length + 4 - nBase64.Length % 4, '=');
                }
                string eBase64 = json.keys[i].e.Replace('-', '+').Replace('_', '/');
                if(eBase64.Length % 4 > 0) {
                    eBase64 = eBase64.PadRight(eBase64.Length + 4 - eBase64.Length % 4, '=');
                }
                _rsaParameters[i] = new RSAKey(
                    json.keys[i].kid,
                    new RSAParameters {
                        Modulus = Convert.FromBase64String(nBase64),
                        Exponent = Convert.FromBase64String(eBase64)
                    }
                );
            }
            base.SwitchArray(_rsaParameters);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return false;
        }
    }
}

public interface IRemoteJwksHub {
    RSAParameters? GetKey(string kid, string iss);
    Task<int> RunAsync(CancellationToken tk);
    Task SetupAsync(List<ProviderInfo> providers);
}

public class RemoteJwksHub : IRemoteJwksHub {
    private class IssuerInfo {
        public string[] audience {get;}
        public string[] jwks {get;}
        public string name {get;}
        public RemoteManager manager {get;}
        public IssuerInfo(string name, string[] audience, string[] jwks, RemoteManager manager) {
            this.name = name;
            this.audience = audience;
            this.jwks = jwks;
            this.manager = manager;
        }
    }
    private readonly Dictionary<string, IssuerInfo> _allowedIssuers;
    private IFetch _fetch;
    public RemoteJwksHub(IFetch fetch) {
        _fetch = fetch;
        _allowedIssuers = new Dictionary<string, IssuerInfo>();
    }

    public async Task SetupAsync(List<ProviderInfo> providers) {
        foreach(var provider in providers) {
            var configuration = await Provider.GetConfigurationAsync(_fetch, provider.configuration_uri);
            var issuer = Provider.GetIssuerWithoutPrococol(configuration.issuer);
            _allowedIssuers.Add(issuer, new IssuerInfo(
                provider.name,
                provider.audience,
                new string[] { configuration.jwks_uri },
                new RemoteManager(configuration.jwks_uri, _fetch)
            ));
        }
    }

    public async Task<int> RunAsync(CancellationToken tk) {
        Task[] tasks = new Task[_allowedIssuers.Count];
        int index = 0;
        foreach(var issuer in _allowedIssuers) {
            tasks[index++] = issuer.Value.manager.RunAsync(tk);
        }
        await Task.WhenAll(tasks);
        return 0;
    }

    public RSAParameters? GetKey(string kid, string iss) {
        var issuer = Provider.GetIssuerWithoutPrococol(iss);
        if(!_allowedIssuers.ContainsKey(issuer)) {
            return null;
        }
        RSAKey[]? parameters;
        bool isOk = _allowedIssuers[issuer].manager.GetRsaParameters(out parameters);
        if(!isOk) {
            return null;
        }
        foreach(var p in parameters) {
            if(p.Id == kid) {
                return p.Parameters;
            }
        }
        return null;
    }
}