namespace Utility
{
    public class UserParser
    {
        public static bool isValidId(User user)
        {
            if (Ulid.TryParse(user?.id, out var id))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static UserRole getRole(User user)
        {
            if (user.role == "WSP")
            {
                return UserRole.WSP;
            }
            else if (user.role == "FAR")
            {
                return UserRole.FAR;
            }
            else
            {
                return UserRole.UNKNOW;
            }
        }
    }
}