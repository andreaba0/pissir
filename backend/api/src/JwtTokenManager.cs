using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;
using System.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using JWT.Algorithms;
using JWT;
using System.IO;
using JWT.Serializers;
using System.Xml;
using System.Collections.Generic;

public class JwtTokenManager
{
    private static readonly ValidationKey[] tokens = JwtTokenManager.loadTokens();
    private IConfiguration configuration;
    public JwtTokenManager()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.Development.json");
        configuration = builder.Build();
    }

    public static ValidationKey[] loadTokens()
    {

        var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.Development.json");
        IConfiguration config = builder.Build();
        ValidationKey[] tks = new ValidationKey[3] {
            new ValidationKey {
                KeyId = "key1",
                Key = File.ReadAllText(config["token:key1"], Encoding.ASCII),
                Expiration = new DateTime(2021, 12, 31)
            },
            new ValidationKey {
                KeyId = "key2",
                Key = File.ReadAllText(config["token:key2"], Encoding.ASCII),
                Expiration = new DateTime(2021, 12, 31)
            },
            new ValidationKey {
                KeyId = "key3",
                Key = File.ReadAllText(config["token:key3"], Encoding.ASCII),
                Expiration = new DateTime(2021, 12, 31)
            }
        };

        return tks;
    }

    public static TokenOut jwtVerified(string token)
    {
        try
        {
            List<SecurityKey> keys = new List<SecurityKey>();
            foreach (var tk in tokens)
            {
                var rsa = RSA.Create();
                rsa.ImportFromPem(tk.Key.ToCharArray());
                RSAParameters rsaParameters = rsa.ExportParameters(false);
                keys.Add(new RsaSecurityKey(rsaParameters)
                {
                    KeyId = tk.KeyId
                });
            }
            var validationParameters = new TokenValidationParameters
            {
                RequireExpirationTime = false,
                RequireSignedTokens = true,
                ValidateAudience = false,
                ValidateIssuer = false,
                IssuerSigningKeys = keys
            };
            var handler = new JwtSecurityTokenHandler();
            ClaimsPrincipal principal = handler.ValidateToken(token, validationParameters, out var validatedToken);
            //parse claims in dictionary
            Dictionary<string, string> claims = new Dictionary<string, string>();
            /*foreach (var claim in principal.Claims)
            {
                Console.WriteLine(claim.Type);
                Console.WriteLine(claim.Value);
                claims.Add(claim.Type, claim.Value);
            }*/
            if(!principal.HasClaim(c => c.Type == ClaimTypes.Role))
            {
                return new TokenOut
                {
                    success = false,
                    error = "Missing role claim"
                };
            }
            var roles = principal.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();
            return new TokenOut
            {
                success = true,
                principal = principal,
                claims = claims
            };

        }
        catch(SecurityTokenInvalidSignatureException e) {
            return new TokenOut
            {
                success = false,
                error = "Invalid signature"
            };
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Console.WriteLine(e.GetType().ToString());
            return new TokenOut
            {
                success = false,
                error = "Generic error"
            };
        }

        return new TokenOut
        {
            success = true
        };
    }
}

public class ValidationKey
{
    public string KeyId { get; set; }
    public string Key { get; set; }
    public DateTime? Expiration { get; set; }
}

public class TokenOut
{
    public bool success = true;
    public string? error = null;
    public ClaimsPrincipal? principal = null;
    public Dictionary<string, string>? claims = null;
}