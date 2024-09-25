using Utility;
using Jose;
using System.Text.Json;
using System.Security.Cryptography;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Text;
using Module.KeyManager;
using System;
using System.Text.RegularExpressions;
using Types;

namespace Middleware;

public static class Authentication
{

    internal static string ToBase64Url(byte[] input)
    {
        return Convert.ToBase64String(input)
            .Replace('+', '-')
            .Replace('/', '_')
            .Replace("=", "");
    }

    internal static IDictionary<string, object> ToDictionary(object obj)
    {
        return JsonSerializer.Deserialize<IDictionary<string, object>>((string)obj, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip
        }) ?? new Dictionary<string, object>();
    }

    internal static string GetValue(IDictionary<string, object> dictionary, string key)
    {
        return key switch
        {
            "iss" => dictionary.ContainsKey(key) ? dictionary[key].ToString() : throw new AuthenticationException(AuthenticationException.ErrorCode.ISSUER_REQUIRED, "Issuer required"),
            "aud" => dictionary.ContainsKey(key) ? dictionary[key].ToString() : throw new AuthenticationException(AuthenticationException.ErrorCode.AUDIENCE_REQUIRED, "Audience required"),
            "exp" => dictionary.ContainsKey(key) ? dictionary[key].ToString() : throw new AuthenticationException(AuthenticationException.ErrorCode.EXPIRATION_REQUIRED, "Expiration required"),
            "iat" => dictionary.ContainsKey(key) ? dictionary[key].ToString() : throw new AuthenticationException(AuthenticationException.ErrorCode.IAT_REQUIRED, "Issued at required"),
            "alg" => dictionary.ContainsKey(key) ? dictionary[key].ToString() : throw new AuthenticationException(AuthenticationException.ErrorCode.ALGORITHM_REQUIRED, "Algorithm required"),
            _ => dictionary.ContainsKey(key) ? dictionary[key].ToString() : throw new AuthenticationException(AuthenticationException.ErrorCode.INVALID_TOKEN, $"Unknown key {key} in token")
        };
    }

    internal static Token VerifiedPayload(
        string id_token,
        RemoteManager remoteManager,
        IDateTimeProvider dateTimeProvider
    )
    {
        try
        {
            var parts = id_token.Split('.');
            if (parts.Length != 3)
            {
                throw new AuthenticationException(AuthenticationException.ErrorCode.INVALID_TOKEN, "Malformed token");
            }
            IDictionary<string, object> header = Jose.JWT.Headers(id_token);
            IDictionary<string, object> payload = ToDictionary(Encoding.UTF8.GetString(Base64Url.Decode(parts[1])));
            string signature = parts[2];
            string alg = Authentication.GetValue(header, "alg");
            string kid = Authentication.GetValue(header, "kid");
            string iss = Authentication.GetValue(payload, "iss");
            string aud = Authentication.GetValue(payload, "aud");
            string allowed_iss = EnvManager.Get(EnvManager.Variable.PISSIR_ISS);
            string allowed_aud = EnvManager.Get(EnvManager.Variable.PISSIR_AUD);
            if (iss != allowed_iss) throw new AuthenticationException(AuthenticationException.ErrorCode.INVALID_ISSUER, "Invalid issuer");
            if (aud != allowed_aud) throw new AuthenticationException(AuthenticationException.ErrorCode.INVALID_AUDIENCE, "Invalid audience");
            var key = remoteManager.GetKey(kid);
            string json = Jose.JWT.Decode(id_token, new Jwk(
                Authentication.ToBase64Url(key.Exponent),
                Authentication.ToBase64Url(key.Modulus)
            ), JwsAlgorithm.RS256);

            Token payloadVerified = JsonSerializer.Deserialize<Token>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (Authentication.IsExpired(
                payloadVerified,
                dateTimeProvider
            )) throw new AuthenticationException(AuthenticationException.ErrorCode.TOKEN_EXPIRED, "Token expired");
            if (!Authentication.IsActuallyValid(
                payloadVerified,
                dateTimeProvider
            )) throw new AuthenticationException(AuthenticationException.ErrorCode.INVALID_TOKEN, "Invalid token");
            return payloadVerified;
        }
        catch (AuthenticationException)
        {
            throw;
        }
        catch (JsonException e)
        {
            throw new AuthenticationException(AuthenticationException.ErrorCode.INVALID_TOKEN, "Invalid token", e);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new AuthenticationException(AuthenticationException.ErrorCode.INVALID_TOKEN, "Invalid token", e);
        }
    }

    public static bool IsExpired(Token token, IDateTimeProvider dateTimeProvider)
    {
        DateTime now = dateTimeProvider.UtcNow;
        DateTime expiration = dateTimeProvider.FromUnixTime(token.exp);
        Console.WriteLine(now);
        Console.WriteLine(expiration);
        
        return now > expiration;
    }

    public static bool IsActuallyValid(Token token, IDateTimeProvider dateTimeProvider)
    {
        DateTime now = dateTimeProvider.UtcNow;
        DateTime issuedAt = dateTimeProvider.FromUnixTime(token.iat-(60*2));
        return now >= issuedAt;
    }
    public static bool IsExpired(
        int exp, 
        int iat,
        IDateTimeProvider dateTimeProvider
    )
    {
        DateTime now = dateTimeProvider.UtcNow;
        DateTime expiration = dateTimeProvider.FromUnixTime(exp);
        DateTime issuedAt = dateTimeProvider.FromUnixTime(iat);
        return now >= issuedAt && now <= expiration;
    }
}

public class AuthenticationException : Exception
{
    public enum ErrorCode
    {
        INVALID_TOKEN = 1,
        TOKEN_EXPIRED = 2,
        INVALID_AUDIENCE = 3,
        INVALID_ISSUER = 4,
        INVALID_KID = 5,
        KID_REQUIRED = 6,
        UNSUPPORTED_ALGORITHM = 7,
        INVALID_SIGNATURE = 8,
        ALGORITHM_REQUIRED = 9,
        ISSUER_REQUIRED = 10,
        EXPIRATION_REQUIRED = 11,
        IAT_REQUIRED = 12,
        AUDIENCE_REQUIRED = 13,
        CREDENTIALS_REQUIRED = 14,
        GENERIC_ERROR = 0,
        USER_UNAUTHORIZED = 15,
        INCORRECT_AUTHORIZATION_SCHEME = 16,
        INCORRECT_AUTHORIZATION_HEADER = 17,
        MISSING_AUTHORIZATION_TOKEN_IN_HEADER = 18,
        MISSING_AUTHORIZATION_HEADER = 19
    }

    public ErrorCode Code { get; } = default(ErrorCode);
    public AuthenticationException(ErrorCode code, string message) : base(message)
    {
        this.Code = code;
    }
    public AuthenticationException(ErrorCode code, string message, Exception innerException) : base(message, innerException)
    {
        this.Code = code;
    }
}