namespace Types;

public class Token {
    public string iss {get; set;}
    public string sub {get; set;}
    public string aud {get; set;}
    public int exp {get; set;}
    public int iat {get; set;}
    public string company_vat_number {get; set;}
    public string role {get; set;}
    public Token() {
        iss = string.Empty;
        sub = string.Empty;
        aud = string.Empty;
        exp = 0;
        iat = 0;
        company_vat_number = string.Empty;
        role = string.Empty;
    }
}