using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Security.Cryptography;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Interface.Utility;
using Interface.Module.JsonWebToken;
using Types;

namespace Module.JsonWebToken;

public class JwtControl
{

    private readonly IJwtKeyStore _jwtKeyStore;
    private readonly IClockCustom _clockCustom;
    private readonly IFetch _fetch;
    private string _backendKeyStoreUrl;

    public JwtControl(
        IJwtKeyStore jwtKeyStore,
        IClockCustom clockCustom,
        IFetch fetch,
        string backendKeyStoreUrl
    )
    {
        this._jwtKeyStore = jwtKeyStore;
        this._clockCustom = clockCustom;
        this._fetch = fetch;
        this._backendKeyStoreUrl = backendKeyStoreUrl;
    }

    internal bool LifetimeValidator(DateTime? notBefore, DateTime? expires, SecurityToken securityToken, TokenValidationParameters validationParameters)
    {
        if(expires is null) return false;
        return _clockCustom.UtcNow() < expires;
    }

    internal int ReturnMaxAgeFromHeader(HttpResponseHeaders response) {
        string cacheControl = response.GetValues("Cache-Control").FirstOrDefault();
        Regex regex = new Regex(@"/max\-age\=[0-9]+/s");

        if (regex.IsMatch(cacheControl))
        {
            string maxAge = regex.Match(cacheControl).Value;
            maxAge = maxAge.Replace("max-age=", "");
            int maxAgeInt = int.Parse(maxAge);
            return maxAgeInt;
        }
        else
        {
            return 3600;
        }
    }

    internal async Task<bool> updateKeys()
    {
        var response = await _fetch.Get(_backendKeyStoreUrl);
        if (!response.IsSuccessStatusCode) return false;
        var responseString = await response.Content.ReadAsStringAsync();
        int maxAge = ReturnMaxAgeFromHeader(response.Headers);
        JwksResponse jwksData = JsonSerializer.Deserialize<JwksResponse>(responseString);
        _jwtKeyStore.dropKeys();
        foreach(JsonWebKey jwksKey in jwksData.keys) {
            _jwtKeyStore.SetKey(jwksKey.Kid, new RsaSecurityKey(new RSAParameters
            {
                Exponent = Base64UrlEncoder.DecodeBytes(jwksKey.E),
                Modulus = Base64UrlEncoder.DecodeBytes(jwksKey.N)
            }));
        }
        _jwtKeyStore.setExpiration(maxAge);
        return true;
    }

    public async Task<ClaimsPrincipal> GetClaims(string token)
    {
        if (_jwtKeyStore.isExpired())
        {
            bool hasUpdated = await this.updateKeys();
            if (!hasUpdated) return null;
        }
        //get kid from token
        var handler = new JwtSecurityTokenHandler();
        var tokenS = handler.ReadJwtToken(token);
        string kid = tokenS.Header.Kid;
        RsaSecurityKey key = _jwtKeyStore.GetKey(kid);
        if (key == null) return null;
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            LifetimeValidator = this.LifetimeValidator
        };
        try
        {
            var hnd = new JwtSecurityTokenHandler();
            ClaimsPrincipal claims = hnd.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
            var clonedClaims = new ClaimsPrincipal(new ClaimsIdentity(claims.Identity));
            return clonedClaims;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return null;
        }
    }
}