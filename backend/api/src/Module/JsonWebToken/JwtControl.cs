using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Security.Cryptography;
using Interface.Utility;
using Types;
using Jose;
using Module.KeyManager;

namespace Module.JsonWebToken;

public class JwtControl
{
    private readonly IClockCustom _clockCustom;
    private readonly HttpClient _httpClient;
    private string _backendKeyStoreUrl;

    public JwtControl(
        IClockCustom clockCustom,
        HttpClient fetch,
        string backendKeyStoreUrl
    )
    {
        this._clockCustom = clockCustom;
        this._httpClient = fetch;
        this._backendKeyStoreUrl = backendKeyStoreUrl;
    }

    public User GetClaims(
        string token, 
        Manager _jwtKeyStore
    )
    {
        return null;
        /*principal = null;
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
        }*/
    }
}