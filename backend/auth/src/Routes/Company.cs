using System;
using System.Data;
using System.Data.Common;
using Module.KeyManager;
using Module.Middleware;
using System.Collections.Specialized;
using System.Text.Json;
using System.Text.Json.Serialization;
using Module.Openid;
using System.Net;
using Utility;
using Types;

using AuthorizationService = Module.Middleware.Authorization;

namespace Routes;

public class Company
{
    public string? vat_number { get; set; } = default(string);
    public string? industry_sector { get; set; } = default(string);
    public string? company_name { get; set; } = default(string);
    public string? working_email_address { get; set; } = default(string);
    public string? working_phone_number { get; set; } = default(string);
    public string? working_address { get; set; } = default(string);

    public static Task<string> GetMethod_CompanyInfo(
        IHeaderDictionary headers,
        DbDataSource dataSource,
        IDateTimeProvider dateTimeProvider,
        IRemoteJwksHub remoteJwksHub
    )
    {
        try
        {
            string bearer_token = headers["Authorization"].Count > 0 ? headers["Authorization"].ToString() : string.Empty;
            string id_token = Authentication.ParseBearerToken(bearer_token);
            Token token = Authentication.VerifiedPayload(id_token, remoteJwksHub, dateTimeProvider);
            string providerName = remoteJwksHub.GetIssuerName(token.iss);
            using DbConnection connection = dataSource.OpenConnection();

            User user = AuthorizationService.GetUser(remoteJwksHub, dataSource, token.sub, providerName).Result;
            if (user.role != "FA" && user.role != "WA") throw new ProfileException(ProfileException.ErrorCode.UNKNOW_USER_ROLE, "Unknow user role");
            string userTable = (user.role == "FA") ? "person_fa" : "person_wa";
            string companyTable = (user.role == "FA") ? "company_far" : "company_wsp";


            DbCommand commandGetCompanyProfile = connection.CreateCommand();
            commandGetCompanyProfile.CommandText = @"
                SELECT
                    company_vat_number,
                    company_name,
                    working_email_address,
                    working_phone_number,
                    working_address
                FROM
                    " + userTable + @"
                    inner join company on " + userTable + @".company_vat_number = " + companyTable + @".vat_number
                    inner join company on company.vat_number on " + companyTable + @".vat_number = company.vat_number
                WHERE
                    account_id = (SELECT id FROM user_account WHERE sub=$1 and registered_provider=$2) and role_name=$3
            ";
            commandGetCompanyProfile.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, token.sub));
            commandGetCompanyProfile.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, providerName));
            commandGetCompanyProfile.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, user.role));
            Company company = new Company();
            using (DbDataReader reader = commandGetCompanyProfile.ExecuteReader())
            {
                if (!reader.HasRows) throw new CompanyException(CompanyException.ErrorCode.NOT_FOUND, "Company not found");
                if (reader.Read())
                {
                    company.vat_number = reader.GetString(0);
                    company.company_name = reader.GetString(1);
                    company.working_email_address = reader.GetString(2);
                    company.working_phone_number = reader.GetString(3);
                    company.working_address = reader.GetString(4);
                    return Task.FromResult(JsonSerializer.Serialize(company));
                }
            }
            return Task.FromResult(JsonSerializer.Serialize(company));
        }
        catch (CompanyException)
        {
            throw;
        }
        catch (AuthenticationException)
        {
            throw;
        }
        catch (Exception)
        {
            throw;
        }
    }

    public static Task PatchMethod_CompanyInfo(
        IHeaderDictionary headers,
        System.IO.Stream body,
        DbDataSource dataSource,
        IDateTimeProvider dateTimeProvider,
        IRemoteJwksHub remoteJwksHub
    )
    {
        try
        {
            string bearer_token = headers["Authorization"].Count > 0 ? headers["Authorization"].ToString() : string.Empty;
            string id_token = Authentication.ParseBearerToken(bearer_token);
            Token token = Authentication.VerifiedPayload(id_token, remoteJwksHub, dateTimeProvider);
            string providerName = remoteJwksHub.GetIssuerName(token.iss);
            //expect body to be a json of type Company
            Company company = JsonSerializer.Deserialize<Company>(body) ?? throw new CompanyException(CompanyException.ErrorCode.INVALID_REQUEST_BODY, "Invalid request body");
            using DbConnection connection = dataSource.OpenConnection();

            DbCommand commandGetCompanyInfo = connection.CreateCommand();

            List<string> updateList = new List<string>();
            //foreach property in company add to updateList a string in format "property_name=$1"
            int index = 1;
            if (company.company_name != null)
            {
                updateList.Add("company_name=$" + index);
                commandGetCompanyInfo.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, company.company_name));
                index++;
            }
            if (company.working_email_address != null)
            {
                updateList.Add("working_email_address=$" + index);
                commandGetCompanyInfo.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, company.working_email_address));
                index++;
            }
            if (company.working_phone_number != null)
            {
                updateList.Add("working_phone_number=$" + index);
                commandGetCompanyInfo.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, company.working_phone_number));
                index++;
            }
            if (company.working_address != null)
            {
                updateList.Add("working_address=$" + index);
                commandGetCompanyInfo.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, company.working_address));
                index++;
            }
            if (updateList.Count == 0) throw new CompanyException(CompanyException.ErrorCode.INVALID_REQUEST_BODY, "Request should update at least one field");
            string updateString = string.Join(",", updateList);

            User user = AuthorizationService.GetUser(remoteJwksHub, dataSource, token.sub, providerName).Result;
            if (user.role != "FA" && user.role != "WA") throw new ProfileException(ProfileException.ErrorCode.UNKNOW_USER_ROLE, "Unknow user role");
            string companyTable = (user.role == "FA") ? "company_far" : "company_wsp";
            string userTable = (companyTable == "company_wsp") ? "person_wa" : "person_fa";

            DbCommand commandUpdateCompanyInfo = connection.CreateCommand();
            commandUpdateCompanyInfo.CommandText = @"
                UPDATE
                    company
                SET
                    " + updateString + @"
                WHERE
                    vat_number = (
                        SELECT
                            company_vat_number
                        FROM
                            " + userTable + @"
                        WHERE
                            account_id = (SELECT id FROM user_account WHERE sub=$1 and registered_provider=$2)
                    )
                RETURNING
                    count(company.vat_number)
            ";
            commandUpdateCompanyInfo.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, token.sub));
            commandUpdateCompanyInfo.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, providerName));
            using (DbDataReader reader = commandUpdateCompanyInfo.ExecuteReader())
            {
                if (!reader.HasRows) throw new CompanyException(CompanyException.ErrorCode.NOT_FOUND, "Company not found");
                reader.Read();
                int count = reader.GetInt32(0);
                if (count == 0) throw new CompanyException(CompanyException.ErrorCode.NOT_FOUND, "Company not found");
            }


            return Task.CompletedTask;
        }
        catch (JsonException)
        {
            throw new CompanyException(CompanyException.ErrorCode.INVALID_REQUEST_BODY, "Invalid request body");
        }
        catch (CompanyException)
        {
            throw;
        }
        catch (AuthenticationException)
        {
            throw;
        }
        catch (DbException)
        {
            throw;
        }
        catch (Exception)
        {
            throw new CompanyException(CompanyException.ErrorCode.GENERIC_ERROR, "Generic error");
        }
    }

}

public class CompanyException : Exception
{
    public enum ErrorCode
    {
        GENERIC_ERROR = 0,
        UNKNOW_COMPANY_INDUSTRY_SECTOR = 1,
        UNKNOW_COMPANY_VAT_NUMBER = 2,
        NOT_FOUND = 3,
        INVALID_REQUEST_BODY = 4,
        USER_NOT_FOUND = 5
    }
    public ErrorCode Code { get; } = default(ErrorCode);

    public CompanyException(ErrorCode errorCode, string message) : base(message)
    {
        this.Code = errorCode;
    }

    public CompanyException(ErrorCode errorCode, string message, Exception innerException) : base(message, innerException)
    {
        this.Code = errorCode;
    }

}