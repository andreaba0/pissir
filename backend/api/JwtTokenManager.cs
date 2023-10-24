public class JwtTokenManager {

    private Token[] tokens;
    public JwtTokenManager() {
        
    }

    private async Task<Token[]> loadTokens() {
        return null;
    }
}

class Token {
    public string KeyId { get; set; }
    public string Key { get; set; }
    public DateTime? Expiration { get; set; }
}