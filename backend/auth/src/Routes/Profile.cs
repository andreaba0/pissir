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
            DbCommand commandGetCompanyVatInfo = connection.CreateCommand();
            //TODO get company info from db table
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