using System.Text.RegularExpressions;

namespace Utility
{
    public class User
    {
        public string? role { get; set; }
        public string? email { get; set; }
        public string? name { get; set; }
        public string? surname { get; set; }
        public string? id { get; set; }

        internal static bool isValidName(string name)
        {
            Regex regex = new Regex(@"^[A-Z][a-z]{1,20}$");
            string n = regex.Match(name).Value;
            return n == name;
        }

        internal static bool isValidSurname(string surname)
        {
            Regex regex = new Regex(@"^[A-Z][a-z]{1,20}$");
            string s = regex.Match(surname).Value;
            return s == surname;
        }

        internal static bool isValidEmail(string email)
        {
            Regex regex = new Regex(@"^[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,4}$");
            string e = regex.Match(email).Value;
            return e == email;
        }

        public static bool isValidId(string id) {
            bool isOk = Ulid.TryParse(id, out var uid);
            return isOk;
        }

        public static bool isValidRole(string role) {
            if(role == "WSP" || role == "FAR") {
                return true;
            }
            return false;
        }

        public static Role parseRole(string role) {
            switch(role) {
                case "WSP":
                    return Role.WSP;
                case "FAR":
                    return Role.FAR;
                default:
                    return Role.UNKNOW;
            }
        }
    }
}