using Microsoft.IdentityModel.Tokens;

namespace Interface.Module.JsonWebToken;

public interface IJwtKeyStore {
    public bool isExpired();
    public void setExpiration(int seconds);
    public void SetKey(string id, RsaSecurityKey key);
    public RsaSecurityKey GetKey(string id);
    public void dropKeys();
}