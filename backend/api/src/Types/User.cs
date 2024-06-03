using System.Text.RegularExpressions;
using System.Text.Json;

namespace Types;
public class User
{
    public string? id { get; set; } = string.Empty;
    public string? company_vat_number { get; set; } = string.Empty;
    public string? company_industry_sector { get; set; } = string.Empty;
    public enum IndustrySector
    {
        FA,
        WA,
        UNKNOWN
    }

    public static IndustrySector GetIndustrySector(User user)
    {
        return (user.company_industry_sector) switch
        {
            "FA" => IndustrySector.FA,
            "WA" => IndustrySector.WA,
            _ => IndustrySector.UNKNOWN
        };
    }

    public static bool isValidId(string id)
    {
        bool isOk = Ulid.TryParse(id, out var uid);
        return isOk;
    }

    public static bool HasValidIndustrySector(User user)
    {
        return user.company_industry_sector switch
        { 
            "FA" => true,
            "WA" => true,
            _ => false
        };
    }

    public static User From(AccessToken accessToken)
    {
        User user = new User();
        if (accessToken.sub == default(string)) throw new UserException(UserException.ErrorCode.INVALID_ID, "Invalid id");
        if (accessToken.company_vat_number == default(string)) throw new UserException(UserException.ErrorCode.INVALID_VAT_NUMBER, "Invalid vat number");
        if (accessToken.company_industry_sector == default(string)) throw new UserException(UserException.ErrorCode.INVALID_INDUSTRY_SECTOR, "Invalid industry sector");
        user.id = accessToken.sub;
        user.company_vat_number = accessToken.company_vat_number;
        user.company_industry_sector = accessToken.company_industry_sector;
        return user;
    }
}

public class UserException : Exception
{
    public enum ErrorCode
    {
        GENERIC_ERROR = 0,
        INVALID_ID = 1,
        INVALID_INDUSTRY_SECTOR = 2,
        INVALID_VAT_NUMBER = 3,
        NOT_FOUND = 4
    }
    public ErrorCode Code { get; set; }
    public UserException(ErrorCode code, string message) : base(message)
    {
        this.Code = code;
    }
    public UserException(ErrorCode code, string message, Exception innerException) : base(message, innerException)
    {
        this.Code = code;
    }
}