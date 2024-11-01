namespace Types;

/// <summary>
/// This class is an object used to represent the payload of an authentication token
/// It is encoded with HMACSHA256 and signed with company's secret key
/// Format could be: 
/// </summary>
public class FarmToken {
    public string sub { get; private set; }
    public long iat { get; private set; }
    public long exp { get; private set; }

    // This field is used to make sure that the token cannot be reused for a different URL path
    public string path { get; private set; }
    public string method { get; private set; }
    public FarmToken(string sub, long iat, long exp, string path, string method) {
        this.sub = sub;
        this.iat = iat;
        this.exp = exp;
        this.path = path;
        this.method = method;
    }
}