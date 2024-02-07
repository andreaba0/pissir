using Types;

namespace Middleware;

public static class Authorization {

    public static bool isAuthorized(User user, User.Roles[] roles) {
        foreach (User.Roles role in roles) {
            if (User.ParseRole(user.Role) == role) return true;
        }
        return false;
    }

}