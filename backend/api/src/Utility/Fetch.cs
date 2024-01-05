using Interface.Utility;

namespace Utility;

public class Fetch : IFetch {
    private readonly HttpClient _client;
    public enum Method {
        GET,
        POST,
        PUT,
        DELETE
    }

    public Fetch() {
        this._client = new HttpClient();
        _client.Timeout = TimeSpan.FromSeconds(5);
    }

    public async Task<HttpResponseMessage?> Get(string url) {
        try {
            //fetch the url with a timeout of 5 seconds
            return await _client.GetAsync(url);
        } catch(Exception e) {
            return null;
        }
    }
}