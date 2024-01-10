using System.Net.Http;
using System.Net;
using Interface.Types;

namespace Types;
public class FetchResponseCustom : IFetchResponseCustom {
    public string Content {get;set;}
    public Dictionary<string, object> Headers {get;set;}
    public HttpStatusCode StatusCode {get;set;}
}