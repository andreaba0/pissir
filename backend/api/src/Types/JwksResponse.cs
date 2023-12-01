using Microsoft.IdentityModel.Tokens;

namespace Types;

public class JwksResponse {
    public List<JsonWebKey> keys { get; set; }
}