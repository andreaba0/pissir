namespace Module.JsonWebToken;

public class KeyEndpoint {
    private readonly HttpClient _httpClient;

    public KeyEndpoint(HttpClient httpClient) {
        _httpClient = httpClient;
    }

    public async Task QueryKeys(CancellationToken ct) {
        
    }
}