using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net.Http;
using System.Net;

namespace Module.KeyManager.Openid;

public class OpenidConfiguration {
    public string issuer {get;set;}
    public string authorization_endpoint {get;set;}
    public string jwks_uri {get;set;}
    public string[] claims_supported {get;set;}
    public OpenidConfiguration() {}
}

public class ProviderInfo {
    public string name {get;}
    public string configuration_uri {get;}
    public string[] audience {get;}
    public ProviderInfo(string name, string configuration_uri, string[] audience) {
        this.name = name;
        this.configuration_uri = configuration_uri;
        this.audience = audience;
    }
}

public class Provider {
    private readonly HttpClient _client;
    public Provider(HttpClient client) {
        _client = client;
    }

    public static async Task<OpenidConfiguration> GetConfigurationAsync(HttpClient client, string configuration_uri) {
        var response = await client.GetAsync(configuration_uri);
        if(response.StatusCode != HttpStatusCode.OK) {
            throw new Exception("Failed to fetch openid configuration");
        }
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<OpenidConfiguration>(content, new JsonSerializerOptions {
            PropertyNameCaseInsensitive = true,
            IgnoreNullValues = true,
            //skip unmapped properties
            UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip
        });
    }

    public static string GetIssuerWithoutPrococol(string issuer) {
        var uri = new Uri(issuer);
        return uri.Host + uri.AbsolutePath;
    }
}