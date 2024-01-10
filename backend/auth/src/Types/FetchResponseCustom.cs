using System.Net.Http;
using System.Net;

namespace Types;

public class IFetchResponseCustom {
    public string Content {get;set;}
    public Dictionary<string, object> Headers {get;set;}
    public HttpStatusCode StatusCode {get;set;}
}
public class FetchResponseCustom : IFetchResponseCustom {
    public string Content {get;set;}
    public Dictionary<string, object> Headers {get;set;}
    public HttpStatusCode StatusCode {get;set;}
}