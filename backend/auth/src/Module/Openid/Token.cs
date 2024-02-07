namespace Module.Openid;

public class Token {
    public string iss {get; set;}
    public string sub {get; set;}
    public string aud {get; set;}
    public int exp {get; set;}
    public int iat {get; set;}
    public string given_name {get; set;}
    public string family_name {get; set;}
    public string email {get; set;}
    public bool email_verified {get; set;}
    public Token() {
        iss = string.Empty;
        sub = string.Empty;
        aud = string.Empty;
        exp = 0;
        iat = 0;
        given_name = string.Empty;
        family_name = string.Empty;
        email = string.Empty;
        email_verified = false;
    }
}