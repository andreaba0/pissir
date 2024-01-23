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
        iss = "";
        sub = "";
        aud = "";
        exp = 0;
        iat = 0;
        given_name = "";
        family_name = "";
        email = "";
        email_verified = false;
    }
}