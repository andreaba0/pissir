using Interface.Utility;
using Interface.Types;
using Types;

namespace Utility;

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

    public async Task<HttpResponseMessage?> Get(string url) {
        try {
            //fetch the url with a timeout of 5 seconds
            return await _client.GetAsync(url);
        } catch(Exception e) {
            return null;
        }
    }
    /*public async Task<IFetchResponseCustom?> Get(string url) {
        FetchResponseCustom response = new FetchResponseCustom();
        try {
            var res = await _client.GetAsync(url);
            response.StatusCode = res.StatusCode;
            response.Headers.Add("Cache-Control", res.Headers.CacheControl);
            if(response.IsSuccessStatusCode) {
                var content = await response.Content.ReadAsStringAsync();
                response.Content = content;
            } else {
                response.Content = string.Empty;
            }
            return response;
        } catch(Exception e) {
            return null;
        }
    }*/
}