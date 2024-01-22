using Module.Openid;
using Utility;
using Jose;
using System.Text.Json;
using System.Security.Cryptography;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Text;
using Module.KeyManager;

namespace Module.Middleware;

public static class Authentication
{
    public enum Role
    {
        GSI,
        FAR,
        Anonymous
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
        return JsonSerializer.Deserialize<Dictionary<string, object>>(JsonSerializer.Serialize(obj), new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    internal static string GetValue(IDictionary<string, object> dictionary, string key)
    {
        return key switch
        {
            "iss" => dictionary.ContainsKey(key) ? dictionary[key].ToString() : throw new AuthenticationException(AuthenticationException.ErrorCode.ISSUER_REQUIRED),
            "aud" => dictionary.ContainsKey(key) ? dictionary[key].ToString() : throw new AuthenticationException(AuthenticationException.ErrorCode.AUDIENCE_REQUIRED),
            "exp" => dictionary.ContainsKey(key) ? dictionary[key].ToString() : throw new AuthenticationException(AuthenticationException.ErrorCode.EXPIRATION_REQUIRED),
            "iat" => dictionary.ContainsKey(key) ? dictionary[key].ToString() : throw new AuthenticationException(AuthenticationException.ErrorCode.IAT_REQUIRED),
            "alg" => dictionary.ContainsKey(key) ? dictionary[key].ToString() : throw new AuthenticationException(AuthenticationException.ErrorCode.ALGORITHM_REQUIRED),
            _ => dictionary.ContainsKey(key) ? dictionary[key].ToString() : throw new AuthenticationException(AuthenticationException.ErrorCode.INVALID_TOKEN, $"Unknown key {key} in token")
        };
    }

    internal static Token ParseToken(
        string id_token,
        IRemoteJwksHub remoteManager
    )
    {
        try
        {
            var parts = token.Split('.');
            if (parts.Length != 3)
            {
                throw new AuthenticationException(AuthenticationException.ErrorCode.INVALID_TOKEN);
            }
            IDictionary<string, object> header = ToDictionary(Base64Url.Decode(parts[0]));
            IDictionary<string, object> payload = ToDictionary(Base64Url.Decode(parts[1]));
            string signature = parts[2];
            string alg = Authentication.GetValue(header, "alg");
            string kid = Authentication.GetValue(header, "kid");
            string iss = Authentication.GetValue(payload, "iss");
            string aud = Authentication.GetValue(payload, "aud");
            string exp = Authentication.GetValue(payload, "exp");
            string iat = Authentication.GetValue(payload, "iat");
            bool isValidSignature = Authentication.VerifySignature(
                remoteManager,
                signature,
                Encoding.UTF8.GetBytes($"{parts[0]}.{parts[1]}")
            );
            if (!isValidSignature) throw new AuthenticationException(AuthenticationException.ErrorCode.INVALID_SIGNATURE);
            if (!Authentication.IsExpired(
                int.Parse(exp),
                int.Parse(iat),
                new DateTimeProvider()
            )) throw new AuthenticationException(AuthenticationException.ErrorCode.TOKEN_EXPIRED);
            return JsonSerializer.Deserialize<Token>(JsonSerializer.Serialize(payload), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (AuthenticationException e)
        {
            throw e;
        }
        catch (Exception e)
        {
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
        catch (AuthenticationException e)
        {
            throw e;
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
    public static bool IsExpired(
        int exp,
        int iat,
        IDateTimeProvider dateTimeProvider
    )
    {
        DateTime now = dateTimeProvider.Now;
        DateTime expiration = dateTimeProvider.FromUnixTime(exp);
        DateTime issuedAt = dateTimeProvider.FromUnixTime(iat);
        return now >= issuedAt && now <= expiration;
    }
    public static Token ReturnValidatedUser(
        string id_token,
        IRemoteJwksHub remoteJwksHub,
        IDateTimeProvider dateTimeProvider
    )
    {
        try
        {
            Token token = Authentication.VerifySignature(
                remoteJwksHub,
                id_token
            );
            bool hasExpired = Authentication.ValidateExpiration(
                token,
                dateTimeProvider
            );
            if (hasExpired)
            {
                throw new AuthenticationException(AuthenticationException.ErrorCode.TOKEN_EXPIRED);
            }
            if ()
                return token;
        }
        catch (AuthenticationException e)
        {
            throw e;
        }
        catch (Exception e)
        {
            throw new AuthenticationException(AuthenticationException.ErrorCode.INVALID_TOKEN, "Invalid token", e);

        }
    }
}

public class AuthenticationException : Exception
{
    public enum ErrorCode
    {
        INVALID_TOKEN,
        TOKEN_EXPIRED,
        INVALID_AUDIENCE,
        INVALID_ISSUER,
        INVALID_KID,
        KID_REQUIRED,
        UNSUPPORTED_ALGORITHM,
        INVALID_SIGNATURE,
        ALGORITHM_REQUIRED,
        ISSUER_REQUIRED,
        EXPIRATION_REQUIRED,
        IAT_REQUIRED,
        AUDIENCE_REQUIRED
    }

    public ErrorCode Code { get; }
    public AuthenticationException(ErrorCode code)
    {
        this.Code = code;
    }
    public AuthenticationException(ErrorCode code, string message) : base(message)
    {
        this.Code = code;
    }
    public AuthenticationException(ErrorCode code, string message, Exception innerException) : base(message, innerException)
    {
        this.Code = code;
    }
}