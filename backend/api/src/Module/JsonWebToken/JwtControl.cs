using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Security.Cryptography;
using Interface.Utility;
using Interface.Module.JsonWebToken;
using Types;
using Jose;

namespace Module.JsonWebToken;

public class JwtControl
{
    private readonly IClockCustom _clockCustom;
    private readonly IFetch _fetch;
    private string _backendKeyStoreUrl;

    public JwtControl(
        IClockCustom clockCustom,
        IFetch fetch,
        string backendKeyStoreUrl
    )
    {
        this._clockCustom = clockCustom;
        this._fetch = fetch;
        this._backendKeyStoreUrl = backendKeyStoreUrl;
    }

    public bool GetClaims(
        string token, 
        out IdentityToken principal, 
        out string error_message,
        IKeyService _jwtKeyStore
    )
    {
        principal = null;
        error_message = string.Empty;
        
        IDictionary<string, object> headers = Jose.JWT.Headers(token);
        if (!headers.ContainsKey("kid")) {
            error_message = "Missing kid";
            return false;
        }
        string kid = headers["kid"].ToString();

        RSAParameters? key = _jwtKeyStore.GetKey(kid);
        if (key == null) {
            error_message = "Key not found";
            return false;
        }
        string json = Jose.JWT.Decode(token, key, JwsAlgorithm.RS256);
        try {
            principal = JsonSerializer.Deserialize<IdentityToken>(json);
            return true;
        } catch(JsonException ex) {
            error_message = "Invalid json";
            return false;
        } catch(ArgumentNullException ex) {
            error_message = "Provide a valid token";
            return false;
        } catch(Exception ex) {
            error_message = "Generic error";
            return false;
        }
    }
}