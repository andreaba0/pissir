using Utility;
using System.Security.Claims;

namespace Middleware;

public static class Authorization {

    public static bool isAuthorized(ClaimsPrincipal claims, Role role) {
        if(claims == null) return false;
        if(role == Role.UNKNOW) return false;
        if(role == Role.WSP&&claims.HasClaim("role", "WSP")) {
            return true;
        }
        if(role == Role.FAR&&claims.HasClaim("role", "FAR")) {
            return true;
        }
        return false;
    }

}