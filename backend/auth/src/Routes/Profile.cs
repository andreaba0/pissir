using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data;
using Module.KeyManager;
using Module.Middleware;
using System.Collections.Specialized;
using Module.Openid;
using System.Net;

using System.Threading.Tasks;
using Utility;
using System.Net.Http;

namespace Routes;

public class Profile
{
    public string id { get; set; } = default(string);
    public string given_name { get; set; } = default(string);
    public string family_name { get; set; } = null;
    public string email { get; set; } = null;
    public string tax_code { get; set; } = null;
    public string role { get; set; } = null;
    public string company_vat_number { get; set; } = null;
    public Profile() { }
    public static Profile GetMethod(
        IHeaderDictionary headers,
        DbDataSource dataSource,
        IDateTimeProvider dateTimeProvider,
        IRemoteJwksHub remoteJwksHub
    )
    {
        try
        {
            Profile profile = new Profile();
            string bearer_token = headers["Authorization"];
            string id_token = default(string);
            bool isBearerToken = Authentication.TryParseBearerToken(bearer_token, out id_token);
            if (!isBearerToken) throw new AuthenticationException(AuthenticationException.ErrorCode.INVALID_TOKEN, "Bearer token required");
            if (id_token == null) throw new AuthenticationException(AuthenticationException.ErrorCode.CREDENTIALS_REQUIRED, "Credentials required");
            Token token = Authentication.ParseToken(id_token, remoteJwksHub, dateTimeProvider);
            using DbConnection connection = dataSource.OpenConnection();
            DbCommand command = connection.CreateCommand();
            command.CommandText = @"
                SELECT 
                    global_id, 
                    given_name, 
                    family_name, 
                    email, 
                    tax_code, 
                    company_vat_number, 
                    industry_sector
                FROM 
                    person 
                        inner join 
                    user_account 
                        on 
                            person.account_id = user_account.id
                        inner join
                    company
                        on
                            person.company_vat_number = company.vat_number
                WHERE user_account.sub=$1 and user_account.registered_provider=$2
            ";
            //NpgSql
            command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, token.sub));
            string providerName = remoteJwksHub.GetIssuerName(token.iss);
            command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, providerName));
            using (DbDataReader reader = command.ExecuteReader())
            {
                if (!reader.HasRows) throw new ProfileException(ProfileException.ErrorCode.USER_NOT_FOUND, "User not found");
                if (reader.Read())
                {
                    profile.id = reader.GetString(0);
                    profile.given_name = reader.GetString(1);
                    profile.family_name = reader.GetString(2);
                    profile.email = reader.GetString(3);
                    profile.tax_code = reader.GetString(4);
                    profile.role = (reader.GetString(5)) switch
                    {
                        "FA" => "FA",
                        "WSP" => "WSP",
                        _ => throw new ProfileException(ProfileException.ErrorCode.UNKNOW_COMPANY_INDUSTRY_SECTOR)
                    };
                    profile.company_vat_number = reader.GetString(6);
                }
                reader.Close();
            }
            connection.Close();
            return profile;
        }
        catch (AuthenticationException)
        {
            throw;
        }
        catch (ProfileException)
        {
            throw;
        }
        catch (DbException e)
        {
            throw;
        }
        catch (Exception e)
        {
            throw new ProfileException(ProfileException.ErrorCode.GENERIC_ERROR, "Generic error", e);
        }
    }
}

public class ProfileException : Exception
{
    public enum ErrorCode
    {
        INVALID_TOKEN,
        CREDENTIALS_REQUIRED,
        INVALID_SIGNATURE,
        TOKEN_EXPIRED,
        GENERIC_ERROR,
        UNKNOW_COMPANY_INDUSTRY_SECTOR,
        USER_NOT_FOUND,
    }
    public ErrorCode Code { get; } = default(ErrorCode);

    public ProfileException(ErrorCode errorCode)
    {
        this.Code = errorCode;
    }
    public ProfileException(ErrorCode errorCode, string message) : base(message)
    {
        this.Code = errorCode;
    }
    public ProfileException(ErrorCode errorCode, string message, Exception innerException) : base(message, innerException)
    {
        this.Code = errorCode;
    }
}