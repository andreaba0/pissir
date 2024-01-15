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
            lock(_lock) {
                if(_rsaParameters != null) {
                    Console.WriteLine("RSA parameters are not null");
                    Console.WriteLine(JsonSerializer.Serialize(_rsaParameters));
                } else {
                    Console.WriteLine("RSA parameters are null");
                }
            }
            while (!await UpdateRsaParameters())
            {
                await Task.Delay(1000, tk);
            }
            await Task.Delay(60 * 5 * 1000, tk);
        }
        return 0;
    }

    protected abstract Task<bool> UpdateRsaParameters();

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

public class RemoteManager : Manager {
    private readonly string _uri;
    private readonly HttpClient _client;
    private static readonly char[] padding = { '=' };
    public RemoteManager(string uri, HttpClient client) : base()
    {
        _uri = uri;
        _client = client;
    }

    public RSAParameters? GetKey(string kid) {
        RSAKey[]? parameters;
        bool isOk = GetRsaParameters(out parameters);
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

    protected override async Task<bool> UpdateRsaParameters() {
        try
        {
            var response = await _client.GetAsync(_uri);
            if(response == null) return false;
            if(response.StatusCode!=HttpStatusCode.OK) return false;
            var content = await response.Content.ReadAsStringAsync();
            KeyArray? json = JsonSerializer.Deserialize<KeyArray>(content, new JsonSerializerOptions {
                PropertyNameCaseInsensitive = true,
                IgnoreNullValues = true,
                UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip
            });
            Console.WriteLine(JsonSerializer.Serialize(json));
            if(json == null) return false;
            RSAKey?[] _rsaParameters = new RSAKey[json.keys.Length];
            Console.WriteLine("Updated RSA parameters");
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
            Console.WriteLine(JsonSerializer.Serialize(_rsaParameters));
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
    private HttpClient _client;
    public RemoteJwksHub(HttpClient client) {
        _client = client;
        _allowedIssuers = new Dictionary<string, IssuerInfo>();
    }

    public async Task SetupAsync(List<ProviderInfo> providers) {
        foreach(var provider in providers) {
            var configuration = await Provider.GetConfigurationAsync(_client, provider.configuration_uri);
            var issuer = Provider.GetIssuerWithoutPrococol(configuration.issuer);
            _allowedIssuers.Add(issuer, new IssuerInfo(
                provider.name,
                provider.audience,
                new string[] { configuration.jwks_uri },
                new RemoteManager(configuration.jwks_uri, _client)
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