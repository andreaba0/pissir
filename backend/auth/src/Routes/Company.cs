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
                    vat_number,
                    company_name,
                    working_email_address,
                    working_phone_number,
                    working_address
                FROM
                    company
                WHERE
                    vat_number = $1
            ";
            commandGetCompanyProfile.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, user.company_vat_number));
            Company company = new Company();
            using (DbDataReader reader = commandGetCompanyProfile.ExecuteReader())
            {
                if (!reader.HasRows) throw new CompanyException(CompanyException.ErrorCode.NOT_FOUND, "Company not found");
                if (reader.Read())
                {
                    company.vat_number = reader.GetString(0);
                    company.company_name = (reader.IsDBNull(1)) ? null : reader.GetString(1);
                    company.working_email_address = (reader.IsDBNull(2)) ? null : reader.GetString(2);
                    company.working_phone_number = (reader.IsDBNull(3)) ? null : reader.GetString(3);
                    company.working_address = (reader.IsDBNull(4)) ? null : reader.GetString(4);
                    company.industry_sector = (user.role == "FA") ? "FAR" : "WSP";
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
            Company company = JsonSerializer.DeserializeAsync<Company>(body).Result ?? throw new CompanyException(CompanyException.ErrorCode.INVALID_REQUEST_BODY, "Invalid request body");
            using DbConnection connection = dataSource.OpenConnection();

            DbCommand commandPatchCompanyInfo = connection.CreateCommand();

            List<string> updateList = new List<string>();
            //foreach property in company add to updateList a string in format "property_name=$1"
            int index = 1;
            if (company.company_name != default(string))
            {
                updateList.Add("company_name=$" + index);
                commandPatchCompanyInfo.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, company.company_name));
                index++;
            }
            if (company.working_email_address != default(string))
            {
                updateList.Add("working_email_address=$" + index);
                commandPatchCompanyInfo.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, company.working_email_address));
                index++;
            }
            if (company.working_phone_number != default(string))
            {
                updateList.Add("working_phone_number=$" + index);
                commandPatchCompanyInfo.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, company.working_phone_number));
                index++;
            }
            if (company.working_address != default(string))
            {
                updateList.Add("working_address=$" + index);
                commandPatchCompanyInfo.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, company.working_address));
                index++;
            }
            if (updateList.Count == 0) throw new CompanyException(CompanyException.ErrorCode.INVALID_REQUEST_BODY, "Request should update at least one field");
            string updateString = string.Join(",", updateList);

            User user = AuthorizationService.GetUser(remoteJwksHub, dataSource, token.sub, providerName).Result;
            if (user.role != "FA" && user.role != "WA") throw new ProfileException(ProfileException.ErrorCode.UNKNOW_USER_ROLE, "Unknow user role");
            string companyTable = (user.role == "FA") ? "company_far" : "company_wsp";
            string userTable = (companyTable == "company_wsp") ? "person_wa" : "person_fa";

            commandPatchCompanyInfo.CommandText = $@"
                UPDATE
                    company
                SET
                    {updateString}
                WHERE
                    vat_number = '{user.company_vat_number}'
                RETURNING
                    company.vat_number
            ";
            using (DbDataReader reader = commandPatchCompanyInfo.ExecuteReader())
            {
                if (!reader.HasRows) throw new CompanyException(CompanyException.ErrorCode.NOT_FOUND, "Company not found");
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
        catch (Exception e)
        {
            Console.WriteLine(e);
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