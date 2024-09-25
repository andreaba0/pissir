namespace Types;

public struct UserFields {
    public string? global_id { get; set;}
    public string? role { get; set;}
    public string? company_vat_number { get; set;}
    public UserFields() {
        global_id = default(string);
        role = default(string);
        company_vat_number = default(string);
    }
}

public class User {
    public readonly string global_id;
    public readonly string role;
    public readonly string company_vat_number;
    //No company industry required as it is obvious that company industry and user role are related

    public enum Role {
        FA,
        WA
    }

    public User(
        string global_id,
        string role,
        string company_vat_number
    ) {
        this.global_id = global_id;
        this.role = role;
        this.company_vat_number = company_vat_number;
    }

    public static Role GetRole(User user) {
        if(user.role == "FA") return Role.FA;
        if(user.role == "WA") return Role.WA;
        throw new UserException(UserException.ErrorCode.UNKNOW_USER_ROLE, "Unknow user role");
    }

    public static User Get(UserFields userStruct) {
        if(userStruct.global_id == default(string)) throw new UserException(UserException.ErrorCode.MISSING_FIELD, "global_id is required");
        if(userStruct.role == default(string)) throw new UserException(UserException.ErrorCode.MISSING_FIELD, "role is required");
        if(userStruct.company_vat_number == default(string)) throw new UserException(UserException.ErrorCode.MISSING_FIELD, "company_vat_number is required");
        return new User(
            userStruct.global_id,
            userStruct.role,
            userStruct.company_vat_number
        );
    }
}

public class UserException : Exception {
    public enum ErrorCode {
        GENERIC_ERROR = 0,
        MISSING_FIELD = 1,
        UNKNOW_USER_ROLE = 2
    }
    public ErrorCode Code { get; } = default(ErrorCode);
    public UserException(ErrorCode errorCode, string message) : base(message) {
        Code = errorCode;
    }
    public UserException(ErrorCode errorCode, string message, Exception innerException) : base(message, innerException) {
        Code = errorCode;
    }
}