using Types;
using System.Net.Http;
using System.Net;

namespace Utility;

public interface IFetch {
    public Task<IFetchResponseCustom> Get(string url);
}
public class Fetch : IFetch {
    private readonly HttpClient _client;
    public enum Method {
        GET,
        POST,
        PUT,
        DELETE
    }
    public enum ContentType {
        JSON,
        TEXT
    }

    public Fetch() {
        this._client = new HttpClient();
        _client.Timeout = TimeSpan.FromSeconds(5);
    }
    public async Task<IFetchResponseCustom> Get(string url) {
        IFetchResponseCustom response = new FetchResponseCustom();
        string content = string.Empty;
        HttpStatusCode statusCode;
        try {
            Console.WriteLine("Fetching " + url);
            var res = await _client.GetAsync(url);
            statusCode = res.StatusCode;
            if(res.StatusCode == HttpStatusCode.OK) {
                content = await res.Content.ReadAsStringAsync();
            }
            response.Headers = new Dictionary<string, object>();
            response.Content = content;
            response.StatusCode = statusCode;
            return response;
        } catch(Exception e) {
            response.Content=string.Empty;
            response.StatusCode=HttpStatusCode.InternalServerError;
            return response;
        }
    }
}