using System.Collections.Generic;
using System.Data.Common;
using System.Data;
using Types;
using Utility;
using Module.KeyManager;

namespace Module.Middleware;

public static class Authorization {
    public static Task<User> GetUser(
        IRemoteJwksHub jwksHub,
        DbDataSource dataSource,
        string sub,
        string providerName
    ) {
        try {
            using DbConnection connection = dataSource.OpenConnection();
            DbCommand command = connection.CreateCommand();
            command.CommandText = @"
                SELECT 
                    global_id, 
                    given_name, 
                    family_name, 
                    email, 
                    tax_code,
                    person_role,
                    company_vat_number
                FROM 
                    person inner join user_account on person.account_id=user_account.id
                WHERE user_account.sub=$1 and user_account.registered_provider=$2
            ";
            //NpgSql
            command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, sub));
            command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, providerName));
            UserFields userFields = new UserFields();
            using DbDataReader reader = command.ExecuteReader();
            if(!reader.HasRows) {
                throw new AuthorizationException(AuthorizationException.ErrorCode.USER_NOT_FOUND, "User not found");
            }
            while(reader.Read()) {
                userFields.global_id = reader.GetString(0);
                userFields.given_name = reader.GetString(1);
                userFields.family_name = reader.GetString(2);
                userFields.email = reader.GetString(3);
                userFields.tax_code = reader.GetString(4);
                userFields.role = reader.GetString(5);
                userFields.company_vat_number = reader.GetString(6);
            }
            return Task.FromResult(User.Get(userFields));
        } catch(Exception) {
            throw;
        }
    }
}

public class AuthorizationException : Exception {
    public enum ErrorCode {
        GENERIC_ERROR = 0,
        INVALID_DATE = 1,
        INVALID_COMPANY_VAT_NUMBER = 2,
        USER_NOT_FOUND = 3,
    }
    public ErrorCode Code { get; } = default(ErrorCode);
    public AuthorizationException(ErrorCode errorCode, string message) : base(message) {
        Code = errorCode;
    }
    public AuthorizationException(ErrorCode errorCode, string message, Exception innerException) : base(message, innerException) {
        Code = errorCode;
    }
}