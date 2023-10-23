public class JwtTokenManager {

    private Token[] tokens;
    public JwtTokenManager() {
        
    }
}

class Token {
    public string KeyId { get; set; }
    public string Key { get; set; }
}