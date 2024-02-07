namespace Types;

public struct UserFields {
    public string? global_id { get; set;}
    public string? given_name { get; set;}
    public string? family_name { get; set;}
    public string? email { get; set;}
    public string? tax_code { get; set;}
    public string? role { get; set;}
    public string? company_vat_number { get; set;}
    public UserFields() {
        global_id = default(string);
        given_name = default(string);
        family_name = default(string);
        email = default(string);
        tax_code = default(string);
        role = default(string);
        company_vat_number = default(string);
    }
}

public class User {
    public string global_id { get; }
    public string given_name { get; }
    public string family_name { get; }
    public string email { get; }
    public string tax_code { get; }
    public string role { get; }
    public string company_vat_number { get; }

    public User(
        string global_id,
        string given_name,
        string family_name,
        string email,
        string tax_code,
        string role,
        string company_vat_number
    ) {
        this.global_id = global_id;
        this.given_name = given_name;
        this.family_name = family_name;
        this.email = email;
        this.tax_code = tax_code;
        this.role = role;
        this.company_vat_number = company_vat_number;
    }

    public static User Get(UserFields userStruct) {
        if(userStruct.global_id == default(string)) throw new UserException(UserException.ErrorCode.MISSING_FIELD, "global_id is required");
        if(userStruct.given_name == default(string)) throw new UserException(UserException.ErrorCode.MISSING_FIELD, "given_name is required");
        if(userStruct.family_name == default(string)) throw new UserException(UserException.ErrorCode.MISSING_FIELD, "family_name is required");
        if(userStruct.email == default(string)) throw new UserException(UserException.ErrorCode.MISSING_FIELD, "email is required");
        if(userStruct.tax_code == default(string)) throw new UserException(UserException.ErrorCode.MISSING_FIELD, "tax_code is required");
        if(userStruct.role == default(string)) throw new UserException(UserException.ErrorCode.MISSING_FIELD, "role is required");
        if(userStruct.company_vat_number == default(string)) throw new UserException(UserException.ErrorCode.MISSING_FIELD, "company_vat_number is required");
        return new User(
            userStruct.global_id,
            userStruct.given_name,
            userStruct.family_name,
            userStruct.email,
            userStruct.tax_code,
            userStruct.role,
            userStruct.company_vat_number
        );
    }
}

public class UserException : Exception {
    public enum ErrorCode {
        GENERIC_ERROR = 0,
        MISSING_FIELD = 1,
    }
    public ErrorCode Code { get; } = default(ErrorCode);
    public UserException(ErrorCode errorCode, string message) : base(message) {
        Code = errorCode;
    }
    public UserException(ErrorCode errorCode, string message, Exception innerException) : base(message, innerException) {
        Code = errorCode;
    }
}