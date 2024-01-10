using Types;
using System.Net.Http;
using System.Net;

namespace Utility;

public interface IFetch {
    public Task<IFetchResponseCustom?> Get(string url);
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
    public async Task<IFetchResponseCustom?> Get(string url) {
        FetchResponseCustom response = new FetchResponseCustom();
        try {
            Console.WriteLine("Fetching " + url);
            var res = await _client.GetAsync(url);
            response.StatusCode = res.StatusCode;
            Console.WriteLine("Status code: " + res.StatusCode);
            //response.Headers.
            //response.Headers.Add("Cache-Control", res.Headers.CacheControl);
            //Console.WriteLine("Cache-Control: " + res.Headers.CacheControl);
            if(res.StatusCode == HttpStatusCode.OK) {
                string content = await res.Content.ReadAsStringAsync();
                response.Content = content.Clone();
                Console.WriteLine(response.Content);
                Console.WriteLine("OK");
            } else {
                response.Content = string.Empty;
            }
            return response;
        } catch(Exception e) {
            Console.WriteLine(e);
            return null;
        }
    }
}