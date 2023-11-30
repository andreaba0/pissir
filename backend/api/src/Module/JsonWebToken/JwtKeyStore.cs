namespace Module.JsonWebToken;

public class JwtKeyStore : IJwtKeyStore {
    private List<RsaSecurityKey> keys = new List<RsaSecurityKey>();
    private DateTime expiration;

    public List<RsaSecurityKey> GetKeys() {
        return keys;
    }

    public void SetKeys(List<RsaSecurityKey> keys, DateTime expiration) {
        this.keys = keys;
        this.expiration = expiration;
    }

    public bool isEmpty() {
        return keys.Count == 0;
    }
}