using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data;
using Module.KeyManager;
using Module.Middleware;
using System.Collections.Specialized;
using Module.Openid;
using System.Net;
using Types;

using System.Threading.Tasks;
using Utility;
using System.Net.Http;

using AuthorizationService = Module.Middleware.Authorization;

namespace Routes;

public class Profile
{
    public string? id { get; set; } = default(string);
    public string? given_name { get; set; } = default(string);
    public string? family_name { get; set; } = default(string);
    public string? email { get; set; } = default(string);
    public string? tax_code { get; set; } = default(string);
    public string? role { get; set; } = default(string);
    public string? company_vat_number { get; set; } = default(string);
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
            string bearer_token = headers["Authorization"].Count > 0 ? headers["Authorization"].ToString() : string.Empty;
            string id_token = Authentication.ParseBearerToken(bearer_token);
            Token token = Authentication.VerifiedPayload(id_token, remoteJwksHub, dateTimeProvider);

            User user = AuthorizationService.GetUser(remoteJwksHub, dataSource, token.sub, remoteJwksHub.GetIssuerName(token.iss)).Result;

            using DbConnection connection = dataSource.OpenConnection();

            profile.id = user.global_id;
            profile.given_name = user.given_name;
            profile.family_name = user.family_name;
            profile.email = user.email;
            profile.tax_code = user.tax_code;
            profile.role = user.role;
            profile.company_vat_number = user.company_vat_number;

            if(profile.role != "FA" && profile.role != "WA") throw new ProfileException(ProfileException.ErrorCode.UNKNOW_USER_ROLE, "Unknow user role");

            string companyTable = (profile.role == "FA") ? "company_far" : "company_wsp";
            string personTable = (profile.role == "FA") ? "person_fa" : "person_wa";
            Console.WriteLine("TRTFGDFGEHTG");
            using DbCommand commandGetCompanyVatInfo = connection.CreateCommand();
            //TODO get company info from db table
            commandGetCompanyVatInfo.CommandText = $@"
                SELECT
                    {companyTable}.vat_number
                FROM {companyTable} inner join {personTable} on {companyTable}.vat_number = {personTable}.company_vat_number
                    inner join person on person.account_id = {personTable}.account_id
                WHERE person.global_id = $1
            ";
            commandGetCompanyVatInfo.Parameters.Add(DbUtility.CreateParameter(connection, DbType.Guid, Guid.Parse(profile.id)));
            using DbDataReader reader = commandGetCompanyVatInfo.ExecuteReader();
            if (!reader.HasRows) throw new ProfileException(ProfileException.ErrorCode.USER_NOT_FOUND, "User not found");
            reader.Read();
            profile.company_vat_number = reader.GetString(0);
            reader.Close();
            connection.Close();
            return profile;
        }
        catch (AuthenticationException)
        {
            throw;
        }
        catch (AuthorizationException e)
        {
            if (e.Code == AuthorizationException.ErrorCode.USER_NOT_FOUND)
            {
                throw new ProfileException(ProfileException.ErrorCode.USER_NOT_FOUND, "User not found", e);
            }
            throw;
        }
        catch (ProfileException)
        {
            throw;
        }
        catch (DbException)
        {
            throw;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new ProfileException(ProfileException.ErrorCode.GENERIC_ERROR, "Generic error", e);
        }
    }
}

public class ProfileException : Exception
{
    public enum ErrorCode
    {
        GENERIC_ERROR = 0,
        INVALID_TOKEN = 1,
        CREDENTIALS_REQUIRED = 2,
        INVALID_SIGNATURE = 3,
        TOKEN_EXPIRED = 4,
        UNKNOW_COMPANY_INDUSTRY_SECTOR = 5,
        USER_NOT_FOUND = 6,
        UNKNOW_USER_ROLE = 7
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