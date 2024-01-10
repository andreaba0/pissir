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

namespace Module.KeyManager;

public class RSAArrayElement
{
    public string Id { get; set; }
    public RSAParameters Parameters { get; set; }
    public RSAArrayElement(string id, RSAParameters parameters)
    {
        this.Id = id;
        this.Parameters = parameters;
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
    protected RSAArrayElement[] _rsaParameters;
    protected int _expiration;

    public Manager()
    {
        _rsaParameters = new RSAArrayElement[3];
    }

    public async Task<int> RunAsync(CancellationToken tk)
    {
        while (!tk.IsCancellationRequested)
        {
            while (!await UpdateRsaParameters())
            {
                await Task.Delay(1000, tk);
            }
            await Task.Delay(_expiration * 1000, tk);
        }
        return 0;
    }

    internal abstract Task<bool> UpdateRsaParameters();

    public bool GetRsaParameters(out RSAArrayElement[]? parameters)
    {
        var _copy = new RSAArrayElement[this._rsaParameters.Length];
        for (int i = 0; i < _copy.Length; i++)
        {
            if (_rsaParameters[i] == null)
            {
                parameters = null;
                return false;
            }
            _copy[i] = new RSAArrayElement(
                _rsaParameters[i].Id,
                new RSAParameters
                {
                    D = _rsaParameters[i].Parameters.D,
                    DP = _rsaParameters[i].Parameters.DP,
                    DQ = _rsaParameters[i].Parameters.DQ,
                    Exponent = _rsaParameters[i].Parameters.Exponent,
                    InverseQ = _rsaParameters[i].Parameters.InverseQ,
                    Modulus = _rsaParameters[i].Parameters.Modulus,
                    P = _rsaParameters[i].Parameters.P,
                    Q = _rsaParameters[i].Parameters.Q
                }
            );
        }
        parameters = _copy;
        return true;
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
            var parameters = new RSAArrayElement[3];
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
                parameters[index++] = new RSAArrayElement(
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
            for (int i = 0; i < 3; i++)
            {
                _rsaParameters[i] = parameters[i];
            }
            base._expiration=60*5;
            return true;
        }
        catch (Exception ex)
        {
            base._expiration=1;
            return false;
        }
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
            if(response == null) {
                base._expiration=1;
                return false;
            }
            if(response.StatusCode!=HttpStatusCode.OK) {
                base._expiration=1;
                return false;
            }
            KeyArray? json = JsonSerializer.Deserialize<KeyArray>(response.Content);
            if(json == null) {
                base._expiration=1;
                return false;
            }
            _rsaParameters = new RSAArrayElement[json.keys.Length];
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
                _rsaParameters[i] = new RSAArrayElement(
                    json.keys[i].kid,
                    new RSAParameters {
                        Modulus = Convert.FromBase64String(nBase64),
                        Exponent = Convert.FromBase64String(eBase64)
                    }
                );
            }
            base._expiration = 60*5;
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            base._expiration=1;
            return false;
        }
    }

    public RSAArrayElement[] GetKeys()
    {
        return _rsaParameters;
    }
}