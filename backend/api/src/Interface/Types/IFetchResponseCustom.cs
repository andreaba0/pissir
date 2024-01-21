using System.Net;

namespace Interface.Types;

public class IFetchResponseCustom {
    public string Content {get;}
    public Dictionary<string, object> Headers {get;}
    public HttpStatusCode StatusCode {get;}
}