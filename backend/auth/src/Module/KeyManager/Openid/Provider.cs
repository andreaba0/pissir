using Utility;
using Types;

using System.Text.Json;
using System.Text.Json.Serialization;

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
    private readonly IFetch _fetch;
    public Provider(IFetch fetch) {
        _fetch = fetch;
    }

    public static async Task<OpenidConfiguration> GetConfigurationAsync(IFetch fetch, string configuration_uri) {
        IFetchResponseCustom response = await fetch.Get(configuration_uri);
        if(response.StatusCode != System.Net.HttpStatusCode.OK) {
            throw new Exception("Failed to fetch openid configuration");
        }
        return JsonSerializer.Deserialize<OpenidConfiguration>(response.Content, new JsonSerializerOptions {
            PropertyNameCaseInsensitive = true,
            UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip
        });
    }

    public static string GetIssuerWithoutPrococol(string issuer) {
        var uri = new Uri(issuer);
        return uri.Host + uri.AbsolutePath;
    }
}