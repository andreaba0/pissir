using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Data.Common;
using System.Data;
using System.Text.Json;

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
    public static string Serialize(KeyJson keyJson)
    {
        return JsonSerializer.Serialize(keyJson);
    }
}
public class Manager
{
    private readonly DbDataSource _dbDataSource;
    private RSAArrayElement[] _rsaParameters;

    public Manager(
        DbDataSource dbDataSource
    )
    {
        _dbDataSource = dbDataSource;
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

            await Task.Delay(5000, tk);
        }
        return 0;
    }

    internal async Task<bool> UpdateRsaParameters()
    {
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
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    public bool GetRsaParameters(out RSAArrayElement[] parameters)
    {
        var _copy = new RSAArrayElement[3];
        for (int i = 0; i < 3; i++)
        {
            Console.WriteLine(_rsaParameters[i].Id);
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
        Console.WriteLine(parameters[0].Id);
        return true;
    }
}