using Module.Openid;
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

namespace Module.Middleware;

public static class Authentication
{
    public enum Role
    {
        GSI,
        FAR,
        Anonymous
    }

    public static bool TryParseBearerToken(string authorizationHeader, out string jwt)
    {
        jwt = string.Empty;
        if (authorizationHeader == null || authorizationHeader == string.Empty)
        {
            return false;
        }
        Regex tokenRegex = new Regex(@"^(?<scheme>[A-Za-z-]+)\s(?<token>[A-Za-z0-9-_\.]+)$");
        if (!tokenRegex.IsMatch(authorizationHeader))
        {
            return false;
        }
        string scheme = tokenRegex.Match(authorizationHeader).Groups["scheme"].Value;
        if (scheme.ToLower() != "bearer")
        {
            return false;
        }
        string token = tokenRegex.Match(authorizationHeader).Groups["token"].Value;
        Regex jwtRegex = new Regex(@"^[A-Za-z0-9-_]+\.[A-Za-z0-9-_]+\.[A-Za-z0-9-_]+$");
        string jwtToken = jwtRegex.Match(token).Value;
        if (jwtToken == string.Empty)
        {
            return false;
        }
        if ($"{scheme} {jwtToken}" != authorizationHeader)
        {
            return false;
        }
        jwt = jwtToken;
        return true;
    }

    public static string ParseBearerToken(string authorizationHeader)
    {
        if (authorizationHeader == null || authorizationHeader == string.Empty)
            throw new AuthenticationException(AuthenticationException.ErrorCode.MISSING_AUTHORIZATION_HEADER, "Missing authorization header");
        Regex tokenRegex = new Regex(@"^(?<scheme>[A-Za-z-]+)\s(?<token>[A-Za-z0-9-_\.]+)$");
        if (!tokenRegex.IsMatch(authorizationHeader))
            throw new AuthenticationException(AuthenticationException.ErrorCode.INCORRECT_AUTHORIZATION_HEADER, "Incorrect authorization header");
        string scheme = tokenRegex.Match(authorizationHeader).Groups["scheme"].Value;
        if (scheme.ToLower() != "bearer")
            throw new AuthenticationException(AuthenticationException.ErrorCode.INCORRECT_AUTHORIZATION_SCHEME, "Incorrect authorization scheme");
        string token = tokenRegex.Match(authorizationHeader).Groups["token"].Value;
        Regex jwtRegex = new Regex(@"^[A-Za-z0-9-_]+\.[A-Za-z0-9-_]+\.[A-Za-z0-9-_]+$");
        string jwtToken = jwtRegex.Match(token).Value;
        if (jwtToken == string.Empty)
            throw new AuthenticationException(AuthenticationException.ErrorCode.MISSING_AUTHORIZATION_TOKEN_IN_HEADER, "Missing authorization token in header");
        if ($"{scheme} {jwtToken}" != authorizationHeader)
            throw new AuthenticationException(AuthenticationException.ErrorCode.INCORRECT_AUTHORIZATION_HEADER, "Incorrect authorization header");
        return jwtToken;
    }

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
        IRemoteJwksHub remoteManager,
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
            var key = remoteManager.GetKey(kid, iss);
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

    internal static Token ParseToken(
        string id_token,
        IRemoteJwksHub remoteManager,
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
            string exp = Authentication.GetValue(payload, "exp");
            string iat = Authentication.GetValue(payload, "iat");
            bool isValidSignature = Authentication.VerifySignature(
                kid,
                alg,
                iss,
                remoteManager,
                signature,
                Encoding.UTF8.GetBytes($"{parts[0]}.{parts[1]}")
            );
            if (!isValidSignature) throw new AuthenticationException(AuthenticationException.ErrorCode.INVALID_SIGNATURE, "Invalid signature");
            if (!Authentication.IsExpired(
                int.Parse(exp),
                int.Parse(iat),
                dateTimeProvider
            )) throw new AuthenticationException(AuthenticationException.ErrorCode.TOKEN_EXPIRED, "Token expired");
            return JsonSerializer.Deserialize<Token>(JsonSerializer.Serialize(payload), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
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

    public static bool VerifySignature(
        string kid,
        string alg,
        string iss,
        IRemoteJwksHub remoteManager,
        string signature,
        byte[] data
    )
    {

        try
        {
            var key = remoteManager.GetKey(kid, iss);
            RsaUsingSha rsaUsingSha = new RsaUsingSha("SHA256");
            return rsaUsingSha.Verify(
                Base64Url.Decode(signature),
                data,
                (Jwk)new Jwk(
                    Authentication.ToBase64Url(key.Exponent),
                    Authentication.ToBase64Url(key.Modulus)
            ));
        }
        catch (AuthenticationException)
        {
            throw;
        }
        catch (RemoteJwksHubException e)
        {
            switch (e.Code)
            {
                case RemoteJwksHubException.ErrorCode.KEY_NOT_FOUND:
                    throw new AuthenticationException(AuthenticationException.ErrorCode.INVALID_KID, "Invalid kid", e);
                case RemoteJwksHubException.ErrorCode.ISSUER_NOT_FOUND:
                    throw new AuthenticationException(AuthenticationException.ErrorCode.INVALID_ISSUER, "Invalid issuer", e);
                default:
                    throw new AuthenticationException(AuthenticationException.ErrorCode.INVALID_TOKEN, "Invalid token", e);
            }

        }
        catch (Exception e)
        {
            throw new Exception("Invalid token", e);
        }
    }

    public static bool IsExpired(Token token, IDateTimeProvider dateTimeProvider)
    {
        DateTime now = dateTimeProvider.UtcNow;
        DateTime expiration = dateTimeProvider.FromUnixTime(token.exp);
        return now > expiration;
    }

    public static bool IsActuallyValid(Token token, IDateTimeProvider dateTimeProvider)
    {
        DateTime now = dateTimeProvider.UtcNow;
        DateTime issuedAt = dateTimeProvider.FromUnixTime(token.iat);
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
        MISSING_AUTHORIZATION_HEADER = 19,
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