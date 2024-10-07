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

    internal static void CheckFieldPresence(IDictionary<string, object> dictionary) {
        foreach(var field in typeof(Token).GetProperties()) {
            if(!dictionary.ContainsKey(field.Name)) {
                throw new AuthenticationException(AuthenticationException.ErrorCode.INVALID_TOKEN, $"Missing field(s) in token");
            }
        }
    }

    internal static void CheckAlgAndKidPresence(IDictionary<string, object> dictionary)
    {
        if (!dictionary.ContainsKey("alg")) throw new AuthenticationException(AuthenticationException.ErrorCode.ALGORITHM_REQUIRED, "Algorithm required");
        if (!dictionary.ContainsKey("kid")) throw new AuthenticationException(AuthenticationException.ErrorCode.KID_REQUIRED, "Key ID required");
    }

    /// <summary>
    /// Verify a JWT token
    /// </summary>
    /// <param name="id_token">a jwt token</param>
    /// <param name="remoteManager">An object that store verification keys</param>
    /// <param name="dateTimeProvider">An object that allow to verify token at a specific time</param>
    /// <returns>Token object with the information stored in the token provided as parameter</returns>
    /// <exception cref="AuthenticationException">An exception if an error occurs during token verification</exception>
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
            CheckFieldPresence(payload); // Will throw exception if field is missing based on Token class
            CheckAlgAndKidPresence(header); // Will throw exception if field is missing based on Token class
            string alg = header["alg"].ToString();
            string kid = header["kid"].ToString();
            string iss = payload["iss"].ToString();
            string aud = payload["aud"].ToString();
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
            )) throw new AuthenticationException(AuthenticationException.ErrorCode.INVALID_TOKEN, "Token issued in the future");
            return payloadVerified;
        }
        catch (AuthenticationException)
        {
            throw;
        }
        catch (JsonException e)
        {
            Console.WriteLine(e);
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
        long epoch = (long)DateTimeProvider.epoch(dateTimeProvider.UtcNow);
        int expiration = token.exp;
        Console.WriteLine($"Epoch: {epoch}, Expiration: {expiration}");
        return epoch > expiration;
    }

    public static bool IsActuallyValid(Token token, IDateTimeProvider dateTimeProvider)
    {
        long now = (long)DateTimeProvider.epoch(dateTimeProvider.UtcNow);
        int issuedAt = token.iat;
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