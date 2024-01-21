using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.Json;

using Interface.Module.JsonWebToken;
using Interface.Utility;

using Utility;

namespace Module.JsonWebToken;

internal class RSAPublicKey
{
    public string alg { get; set; }
    public string kid { get; set; }
    public string n { get; set; }
    public string e { get; set; }
    public string use { get; set; }
    public string kty { get; set; }
    public static RSAParameters parse(RSAPublicKey key)
    {
        return new RSAParameters
        {
            Modulus = Convert.FromBase64String(key.n),
            Exponent = Convert.FromBase64String(key.e)
        };
    }
}
internal class RSASignResponse
{
    public RSAPublicKey[] keys { get; set; }
    public RSASignResponse(RSAPublicKey[] keys)
    {
        this.keys = keys;
    }
}

internal class KeyStorage
{
    private ConcurrentDictionary<string, RSAParameters> _keys;
    public KeyStorage(
        RSAPublicKey[] keys
    )
    {
        _keys = new ConcurrentDictionary<string, RSAParameters>();
        foreach (RSAPublicKey key in keys)
        {
            RSAParameters parameters = RSAPublicKey.parse(key);
            _keys.AddOrUpdate(key.kid, parameters, (key, value) => parameters);
        }
    }
    public RSAParameters? GetKey(string kid)
    {
        try {
            return _keys[kid];
        } catch (Exception e) {
            Console.WriteLine(e);
            return null;
        }
    }
}

public class KeyService : IKeyService
{
    private string _backendKeyUri;
    private KeyStorage _keyStorage;
    private IFetch _fetch;
    public KeyService(
        string backendKeyUri,
        IFetch fetch
    )
    {
        _backendKeyUri = backendKeyUri;
        _fetch = fetch;
        _keyStorage = new KeyStorage(new RSAPublicKey[] { });
    }

    public async Task<int> RunAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {

            try
            {
                while (!await UpdateRsaParameters())
                {
                    Console.WriteLine("Failed to update rsa parameters");
                    await Task.Delay(1000);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            await Task.Delay(5000);
        }
        return 0;
    }

    internal async Task<bool> UpdateRsaParameters()
    {
        //make an http GET request to _backendKeyUri and parse the response
        HttpResponseMessage response = await _fetch.Get(_backendKeyUri);
        if (response == null)
        {
            return false;
        }
        if (!response.IsSuccessStatusCode)
        {
            return false;
        }
        string json = await response.Content.ReadAsStringAsync();
        RSASignResponse? signResponse = JsonSerializer.Deserialize<RSASignResponse>(json);
        if (signResponse == null)
        {
            return false;
        }
        KeyStorage keyStorage = new KeyStorage(signResponse.keys);
        Interlocked.Exchange(ref _keyStorage, keyStorage);
        return true;
    }

    public RSAParameters? GetKey(string kid)
    {
        return _keyStorage.GetKey(kid);
    }
}