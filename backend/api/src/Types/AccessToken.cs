using System.Collections.Generic;
using System.Text.Json;

using Utility;

namespace Types;

public class AccessToken
{
    public string? sub { get; set; } = default(string);
    public string? iss { get; set; } = default(string);
    public string? aud { get; set; } = default(string);
    public string? exp { get; set; } = default(string);
    public string? iat { get; set; } = default(string);
    public string? company_vat_number { get; set; } = default(string);
    public string? company_industry_sector { get; set; } = default(string);

    public static Task VerifySignature(
        string signature,
        
        IRemoteJwksHub remoteJwksHub,
        IDateTimeProvider dateTimeProvider
    ) {
        
    }
}