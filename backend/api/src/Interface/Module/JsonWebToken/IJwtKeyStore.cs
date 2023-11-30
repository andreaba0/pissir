namespace Interface.Module.JsonWebToken;

public interface IJwtKeyStore {
    public List<RsaSecurityKey> GetKeys();
    public void SetKeys(List<RsaSecurityKey> keys, DateTime expiration);
    public bool isEmpty();
}