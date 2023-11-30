namespace Utility;

public class Fetch : IFetch {
    private readonly HttpClient _client;

    public Fetch() {
        this._client = new HttpClient();
    }

    public async Task<FetchResponse> Get(string url) {
        var response = await _client.GetAsync(url);
        var responseString = await response.Content.ReadAsStringAsync();
        if(response.IsSuccessStatusCode) {
            string cacheControl = response.Headers.GetValues("Cache-Control").FirstOrDefault();
            Regex regex = new Regex(@"/max\-age\=[0-9]+/s");

            if (regex.IsMatch(cacheControl)) {
                string maxAge = regex.Match(cacheControl).Value;
                maxAge = maxAge.Replace("max-age=", "");
                int maxAgeInt = int.Parse(maxAge);
                return new FetchResponse {
                    ExpiresAt = DateTime.Now.AddSeconds(maxAgeInt),
                    Content = responseString
                };
            } else {
                return new FetchResponse {
                    ExpiresAt = DateTime.Now + TimeSpan.FromSeconds(3600),
                    Content = responseString
                };
            }
            
        }
        return new FetchResponse {
            ExpiresAt = DateTime.Now + TimeSpan.FromSeconds(3600),
            Content = responseString
        };
    }
}