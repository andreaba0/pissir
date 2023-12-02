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

        public static Role getRole(User user)
        {
            if (user.role == "WSP")
            {
                return Role.WSP;
            }
            else if (user.role == "FAR")
            {
                return Role.FAR;
            }
            else
            {
                return Role.UNKNOW;
            }
        }
    }
}