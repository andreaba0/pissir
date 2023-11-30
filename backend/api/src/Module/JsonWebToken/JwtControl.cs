namespace Module.JsonWebToken;

public class JwtControl {

    private readonly IJwtKeyStore _jwtKeyStore;
    private readonly IClockCustom _clockCustom;
    private readonly IFetch _fetch;
    private string _backendKeyStoreUrl;

    public JwtControl(
        IJwtKeyStore jwtKeyStore,
        IClockCustom clockCustom,
        IFetch fetch,
        string backendKeyStoreUrl
    ) {
        this._jwtKeyStore = jwtKeyStore;
        this._clockCustom = clockCustom;
        this._fetch = fetch;
        this._backendKeyStoreUrl = backendKeyStoreUrl;
    }

    internal bool LifetimeValidator(DateTime? notBefore, DateTime? expires, SecurityToken securityToken, TokenValidationParameters validationParameters) {
        if (expires != null) {
            if (_clockCustom.UtcNow() < expires) {
                return true;
            }
        }
        return false;
    }

    internal async Task updateKeys() {
        string response = await _fetch.Get(_backendKeyStoreUrl);
        //convert json array to list
        List<RsaVerifyKeyInfo> rsaFetchParameters = JsonConvert.DeserializeObject<List<RsaVerifyKeyInfo>>(response);
        List<RsaSecurityKey> rsaSecurityKeys = new List<RsaSecurityKey>();
        foreach (RsaVerifyKeyInfo rsaFetchParameter in rsaFetchParameters) {
            rsaSecurityKeys.Add(new RsaSecurityKey(new RSAParameters {
                Exponent = Base64UrlEncoder.DecodeBytes(rsaFetchParameter.e),
                Modulus = Base64UrlEncoder.DecodeBytes(rsaFetchParameter.n)
            }));
        }
        _jwtKeyStore.SetKeys(rsaSecurityKeys, DateTime.Now.AddHours(1));
    }

    public async Task<bool> isValid(string token) {
        if (_jwtKeyStore.isEmpty()) {
            await this.updateKeys();
        }
        if(_jwtKeyStore.isEmpty()) {
            
        }
        var validationParameters = new TokenValidationParameters {
            ValidateIssuerSigningKey = true,
            IssuerSigningKeys = _jwtKeyStore.GetKeys(),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            LifetimeValidator = this.LifetimeValidator
        };
        try {
            var handler = new JwtSecurityTokenHandler();
            handler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
            return true;
        } catch (Exception) {
            return false;
        }
    }
}