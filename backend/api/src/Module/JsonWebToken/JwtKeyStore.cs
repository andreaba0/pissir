using Interface.Module.JsonWebToken;
using Interface.Utility;
using Microsoft.IdentityModel.Tokens;

namespace Module.JsonWebToken;

public class JwtKeyStore : IJwtKeyStore
{
    private readonly object keysLock = new object();
    private Dictionary<string, RsaSecurityKey> keys = new Dictionary<string, RsaSecurityKey>();
    private DateTime expiration;
    private IClockCustom _clock;
    private readonly DateTime defaultExpiration;

    public JwtKeyStore(IClockCustom clock)
    {
        _clock = clock;

        //expiration datetime = 1 1 1970
        defaultExpiration = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        expiration = defaultExpiration;
    }

    public bool isExpired()
    {
        return _clock.Now() > expiration;
    }

    public void setExpiration(int seconds)
    {
        expiration = _clock.Now().AddSeconds(seconds);
    }

    public void SetKey(string id, RsaSecurityKey key)
    {
        lock (keysLock)
        {
            keys[id] = key;
        }
    }

    public RsaSecurityKey GetKey(string id)
    {
        if (isExpired())
        {
            return null;
        }
        if (keys.ContainsKey(id))
        {
            return keys[id];
        }
        else
        {
            return null;
        }
    }

    public void dropKeys()
    {
        keys = new Dictionary<string, RsaSecurityKey>();
    }
}