using Types;
using Module.KeyManager;
using Utility;
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
        {"Farm", Authorization.Scheme.Farm}
    };

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
        if(scheme.ToLower() == "farm" && !Authorization.validateFarmSyntax(tokenRegex, authorizationHeader)) {
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
        Regex jwtRegex = new Regex(@"^[A-Za-z0-9-_]+\.[A-Za-z0-9-_]+\.[A-Za-z0-9-_]+$");
        string jwtToken = jwtRegex.Match(token).Value;
        if(jwtToken == string.Empty) {
            return false;
        }
        return true;
    }

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