using Interface.Utility;

namespace Utility;

public class Fetch : IFetch {
    private readonly HttpClient _client;

    public Fetch() {
        this._client = new HttpClient();
    }

    public async Task<HttpResponseMessage> Get(string url) {
        try {
            return await _client.GetAsync(url);
        } catch(Exception e) {
            Console.WriteLine(e);
            return null;
        }
    }
}