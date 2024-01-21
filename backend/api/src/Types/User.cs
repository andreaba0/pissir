using System.Text.RegularExpressions;
using System.Text.Json;

namespace Types;
public class User
{
    public string Role { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string Id { get; set; } = string.Empty;
    public enum Roles
    {
        WSP,
        FAR,
        UNKNOW
    }
    public enum Fields {
        ROLE,
        EMAIL,
        NAME,
        SURNAME,
        ID
    }

    public bool HasField(Fields[] properties) {
        foreach (Fields property in properties) {
            if (this.HasField(property)) continue;
            return false;
        }
        return true;
    }

    public bool HasField(Fields field) {
        switch(field) {
            case Fields.ROLE:
                return this.Role != string.Empty;
            case Fields.EMAIL:
                return this.Email != string.Empty;
            case Fields.NAME:
                return this.Name != string.Empty;
            case Fields.SURNAME:
                return this.Surname != string.Empty;
            case Fields.ID:
                return this.Id != string.Empty;
            default:
                return false;
        }
    }

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

    public static bool isValidId(string id)
    {
        bool isOk = Ulid.TryParse(id, out var uid);
        return isOk;
    }

    public static bool isValidRole(string role)
    {
        if(User.ParseRole(role) == Roles.UNKNOW) return false;
        return true;
    }

    public static Roles ParseRole(string role)
    {
        return role switch
        {
            "WSP" => Roles.WSP,
            "FAR" => Roles.FAR,
            _ => Roles.UNKNOW
        };
    }

    public User(IdentityToken principal) {
        if(principal.HasClaim(IdentityToken.Claims.SUB)) {
            this.Id = principal.GetClaim(IdentityToken.Claims.SUB).ToString();
        }
        if(principal.HasClaim("Role")) {
            this.Role = principal.GetClaim("Role").ToString();
        }
        if(principal.HasClaim("Email")) {
            this.Email = principal.GetClaim("Email").ToString();
        }
        if(principal.HasClaim("Name")) {
            this.Name = principal.GetClaim("Name").ToString();
        }
        if(principal.HasClaim("Surname")) {
            this.Surname = principal.GetClaim("Surname").ToString();
        }
    }
}