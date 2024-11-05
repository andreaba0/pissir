using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data;
using System.Net.Http;
using System.Net.Http.Headers;
using Types;
using Utility;
using Module.KeyManager;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text;
using Npgsql;
using System.Text.RegularExpressions;

namespace Middleware;

public class AuthorizationObject {
    public Authorization.Types type { get; private set; }
    public string token { get; private set; }
    public AuthorizationObject(Authorization.Types type, string token) {
        this.type = type;
        this.token = token;
    }
}

public class Authorization {
    public enum Types {
        FarmBackend,
        AuthBackend
    }

    public enum Scheme {
        Bearer,
        Internal,
        Farm
    }

    public static readonly Dictionary<string, Authorization.Scheme> _schemeMap = new Dictionary<string, Authorization.Scheme> {
        {"Bearer", Authorization.Scheme.Bearer},
        {"Internal", Authorization.Scheme.Internal},
        {"Pissir-farm-hmac-sha256", Authorization.Scheme.Farm}
    };

    public static bool getAuthorizationHeader(
        IHeaderDictionary headers,
        out string authorizationHeader
    ) {
        authorizationHeader = headers["Authorization"].Count > 0 ? headers["Authorization"].ToString() : string.Empty;
        return authorizationHeader != string.Empty;
    }

    public static bool tryParseAuthorizationHeader(string authorizationHeader, out Authorization.Scheme _scheme, out string _token, out string error_message) {
        _scheme = Authorization.Scheme.Bearer;
        _token = string.Empty;
        error_message = string.Empty;
        if(authorizationHeader == null || authorizationHeader == string.Empty) {
            error_message = "Missing Authorization header";
            return false;
        }
        Regex tokenRegex = new Regex(@"^(?<scheme>[A-Za-z-]+)\s(?<token>[A-Za-z0-9-_\.]+)$");
        if(!tokenRegex.IsMatch(authorizationHeader)) {
            error_message = "Invalid Authorization header";
            return false;
        }
        string scheme = tokenRegex.Match(authorizationHeader).Groups["scheme"].Value;
        if(scheme.ToLower() != "bearer" && scheme.ToLower() != "farm" && scheme.ToLower() != "internal") {
            error_message = $"Only <bearer>, <farm>, and <internal> are supported, but got <{scheme}>";
            return false;
        }
        if(scheme.ToLower() == "bearer" && !Authorization.validateBearerSyntax(tokenRegex, authorizationHeader)) {
            return false;
        }
        if(scheme.ToLower() == "pissir-farm-hmac-sha256" && !Authorization.validateFarmSyntax(tokenRegex, authorizationHeader)) {
            return false;
        }
        string token = tokenRegex.Match(authorizationHeader).Groups["token"].Value;
        if($"{scheme} {token}" != authorizationHeader) {
            error_message = "Invalid token string, expected: \"Authorization: scheme <jwt>\"";
            return false;
        }
        if(!Authorization._schemeMap.ContainsKey(scheme)) {
            error_message = $"Unknown scheme {scheme}";
            return false;
        }
        _scheme = Authorization._schemeMap[scheme];
        _token = token;
        return true;
    }

    public static bool validateBearerSyntax(Regex tokenRegex, string authorizationHeader) {
        string token = tokenRegex.Match(authorizationHeader).Groups["token"].Value;
        Regex jwtRegex = new Regex(@"^[A-Za-z0-9-_]+\.[A-Za-z0-9-_]+\.[A-Za-z0-9-_]+$");
        string jwtToken = jwtRegex.Match(token).Value;
        if(jwtToken == string.Empty) {
            return false;
        }
        return true;
    }

    public static bool validateFarmSyntax(Regex tokenRegex, string authorizationHeader) {
        string token = tokenRegex.Match(authorizationHeader).Groups["token"].Value;
        Regex jwtRegex = new Regex(@"^[A-Za-z0-9-_]+\.[A-Za-z0-9-_]+$");
        string jwtToken = jwtRegex.Match(token).Value;
        if(jwtToken == string.Empty) {
            return false;
        }
        return true;
    }

    /// <summary>
    /// This method is used to limit access to a specific API endpoint based on user role
    /// </summary>
    /// <param name="headers">header of http request</param>
    /// <param name="remoteManager">Remote openid provider where jwt verification keys are queried</param>
    /// <param name="dateTimeProvider">Custom time provider</param>
    /// <param name="roles">List of User.Role that are allowed</param>
    /// <returns>User info contained in jwt access token</returns>
    /// <exception cref="AuthorizationException"></exception>
    public static User AllowByRole(
        IHeaderDictionary headers,
        RemoteManager remoteManager,
        IDateTimeProvider dateTimeProvider,
        List<User.Role> roles
    ) {
        bool ok = false;
        string bearer_token = headers["Authorization"].Count > 0 ? headers["Authorization"].ToString() : string.Empty;
        ok = Authorization.tryParseAuthorizationHeader(bearer_token, out Authorization.Scheme _scheme, out string _token, out string error_message);
        if (!ok)
        {
            throw new AuthorizationException(AuthorizationException.ErrorCode.INVALID_AUTHORIZATION_HEADER, error_message);
        }
        if (_scheme != Authorization.Scheme.Bearer)
        {
            throw new AuthorizationException(AuthorizationException.ErrorCode.BEARER_SCHEME_REQUIRED, "Bearer scheme required");
        }
        Token token = Authentication.VerifiedPayload(_token, remoteManager, dateTimeProvider);
        User user = new User(
            global_id: token.sub,
            role: token.role,
            company_vat_number: token.company_vat_number
        );
        if (!roles.Contains(User.GetRole(user)))
        {
            throw new AuthorizationException(AuthorizationException.ErrorCode.UNAUTHORIZED, "User unauthorized");
        }
        return user;
    }

    /// <summary>
    /// This method is used to return the payload of a resource manager token
    /// Token syntax header follows the format:
    /// "Authorization: Pissir-farm-hmac-sha256 <base64url(payload)>.<base64url(signature)>"
    /// </summary>
    /// <param name="headers"></param>
    /// <param name="dateTimeProvider"></param>
    /// <param name="dataSource"></param>
    /// <returns></returns>
    /// <exception cref="AuthenticationException"></exception>
    /// <exception cref="AuthorizationException"></exception>
    public static FarmToken AuthorizedPayload(
        IHeaderDictionary headers,
        IDateTimeProvider dateTimeProvider,
        DbDataSource dataSource
    ) {
        bool foundAH = Authorization.getAuthorizationHeader(headers, out string authorizationHeader);
        if (!foundAH) {
            throw new AuthenticationException(AuthenticationException.ErrorCode.MISSING_AUTHORIZATION_HEADER, "Missing Authorization header");
        }
        bool parsedAH = Authorization.tryParseAuthorizationHeader(authorizationHeader, out Authorization.Scheme _scheme, out string _token, out string error_message);
        if (!parsedAH) {
            throw new AuthorizationException(AuthorizationException.ErrorCode.INVALID_AUTHORIZATION_HEADER, error_message);
        }
        string[] parts = _token.Split(".");
        string encoded_payload = parts[0];
        string payload = Utility.Utility.Base64URLDecode(parts[0]);
        string request_signature = Utility.Utility.Base64URLDecode(parts[1]);
        FarmToken farmToken = JsonSerializer.Deserialize<FarmToken>(payload, new JsonSerializerOptions {
            PropertyNameCaseInsensitive = true,
        });
        if(farmToken.method != headers["Method"]&& farmToken.path != headers["Path"]){
            throw new AuthorizationException(AuthorizationException.ErrorCode.INVALID_AUTHORIZATION_HEADER, "Invalid path or method");
        }
        long epochNow = DateTimeProvider.epoch(dateTimeProvider.UtcNow);
        if (epochNow > farmToken.exp) {
            throw new AuthenticationException(AuthenticationException.ErrorCode.TOKEN_EXPIRED, "Token expired");
        }
        if (epochNow < farmToken.iat) {
            throw new AuthenticationException(AuthenticationException.ErrorCode.INVALID_TOKEN, "Token issued in the future");
        }
        string vat_number = farmToken.sub;
        using DbConnection connection = dataSource.OpenConnection();
        using DbCommand commandGetSecret = dataSource.CreateCommand();
        commandGetSecret.CommandText = $@"
            select secret_key
            from secret_key
            where vat_number = $1
        ";
        commandGetSecret.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, vat_number));
        using DbDataReader readerSecret = commandGetSecret.ExecuteReader();
        if (!readerSecret.HasRows) {
            throw new AuthorizationException(AuthorizationException.ErrorCode.UNAUTHORIZED, "User unauthorized");
        }
        string secret_key = readerSecret.GetString(0);
        readerSecret.Close();

        string signature = Utility.Utility.HmacSha256(secret_key, encoded_payload);
        if (signature != request_signature) {
            throw new AuthenticationException(AuthenticationException.ErrorCode.INVALID_TOKEN, "Invalid token");
        }
        connection.Close();
        return farmToken;
    }

    public Authorization(string authorizationHeader) {
    }

    /*public static bool isAuthorized(User user, User.Roles[] roles) {
        foreach (User.Roles role in roles) {
            if (User.ParseRole(user.Role) == role) return true;
        }
        return false;
    }*/

}

public class AuthorizationException : Exception {
    public ErrorCode Code { get; private set; }
    public enum ErrorCode {
        GENERIC_ERROR = 0,
        USER_NOT_FOUND = 1,
        AUTHORIZATION_TYPE_UNKNOWN = 2,
        INVALID_AUTHORIZATION_HEADER = 3,
        BEARER_SCHEME_REQUIRED = 4,
        FARM_SCHEME_REQUIRED = 5,
        INTERNAL_SCHEME_REQUIRED = 6,
        INVALID_ROLE = 7,
        UNAUTHORIZED = 8
    }

    public AuthorizationException(string message) : base(message) {}
    public AuthorizationException(ErrorCode code, string message) : base(message) {
        this.Code = code;
    }
}