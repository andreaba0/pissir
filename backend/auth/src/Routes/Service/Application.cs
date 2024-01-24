using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using Module.KeyManager;
using Module.Middleware;
using System.Data.Common;
using System.Data;
using Utility;
using System.Text.Json;
using System.Text.Json.Serialization;
using System;
using Module.Openid;
using System.Net;
using Npgsql;

namespace Routes;

public class Application
{
    public string? id { get; set; } = default(string);
    public string? company_vat_number { get; set; } = default(string);
    public string? given_name { get; set; } = default(string);
    public string? family_name { get; set; } = default(string);
    public string? email { get; set; } = default(string);
    public string? tax_code { get; set; } = default(string);
    public string? company_category { get; set; } = default(string);

    internal static Task UploadApplicationInTransaction(
        DbConnection connection,
        Application application,
        string providerName,
        string sub
    )
    {
        try
        {
            //begin transaction using plain sql command
            DbCommand transactionCommand = connection.CreateCommand();
            transactionCommand.CommandText = "BEGIN TRANSACTION ISOLATION LEVEL READ COMMITTED";
            transactionCommand.ExecuteNonQuery();

            //insert company vat number into company table and if it already exists do nothing
            DbCommand companyCommand = connection.CreateCommand();
            companyCommand.CommandText = @"
                INSERT INTO 
                    company (
                        vat_number, 
                        industry_sector
                    )
                VALUES (
                    $1, 
                    $2
                )
                ON CONFLICT (vat_number) DO NOTHING
            ";
            companyCommand.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, application.company_vat_number));
            companyCommand.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, application.company_category));
            companyCommand.Prepare();
            companyCommand.ExecuteNonQuery();

            //insert user account into user_account table and if it already exists do nothing
            DbCommand userAccountCommand = connection.CreateCommand();
            userAccountCommand.CommandText = @"
                INSERT INTO 
                    user_account (
                        sub, 
                        registered_provider
                    )
                VALUES (
                    $1, 
                    $2
                )
                ON CONFLICT (sub, registered_provider) DO NOTHING
            ";
            userAccountCommand.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, sub));
            userAccountCommand.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, providerName));
            userAccountCommand.Prepare();
            userAccountCommand.ExecuteNonQuery();

            DbCommand command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO 
                    presentation_letter (
                        company_vat_number, 
                        given_name, 
                        family_name, 
                        email, 
                        tax_code, 
                        company_industry_sector, 
                        user_account
                    )
                VALUES (
                    $1, 
                    $2, 
                    $3, 
                    $4, 
                    $5, 
                    $6, 
                    (
                        SELECT 
                            id 
                        FROM 
                            user_account 
                        WHERE 
                            sub=$7 and registered_provider=$8
                    )
                )
                ON CONFLICT (user_account) DO NOTHING
                RETURNING presentation_id
            ";
            command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, application.company_vat_number));
            command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, application.given_name));
            command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, application.family_name));
            command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, application.email));
            command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, application.tax_code));
            command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, application.company_category));
            command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, sub));
            command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, providerName));
            command.Prepare();
            try
            {
                using (DbDataReader dbReader = command.ExecuteReader())
                {
                    if (!dbReader.HasRows) 
                        throw new ApplicationException(ApplicationException.ErrorCode.APPLICATION_ALREADY_EXISTS, "Application already exists");
                    if (dbReader.Read())
                    {
                        application.id = dbReader.GetGuid(0).ToString();
                    }
                }
            }
            catch (NpgsqlException e) when (e.Message.Contains("enforce_company_type"))
            {
                Console.WriteLine(e);
                throw new ApplicationException(ApplicationException.ErrorCode.COMPANY_INDUSTRY_SECTOR_CONFLICT, "Company industry sector conflict", e);
            }
            DbCommand commitCommand = connection.CreateCommand();
            commitCommand.CommandText = "COMMIT";
            commitCommand.ExecuteNonQuery();
            connection.Close();
            return Task.CompletedTask;
        }
        catch (Exception e) when (e.InnerException is not IOException)
        {
            Console.WriteLine(e);
            //rollback transaction
            DbCommand rollbackCommand = connection.CreateCommand();
            rollbackCommand.CommandText = "ROLLBACK";
            rollbackCommand.ExecuteNonQuery();
            throw;
        }
        catch (Exception)
        {
            throw;
        }
    }

    public static Task PostMethod_Apply(
        IHeaderDictionary headers,
        Stream body,
        DbDataSource dataSource,
        IDateTimeProvider dateTimeProvider,
        IRemoteJwksHub remoteJwksHub
     )
    {
        try
        {
            StreamReader reader = new StreamReader(body);
            string bodyString = reader.ReadToEndAsync().Result;
            if (bodyString == null) throw new ApplicationException(ApplicationException.ErrorCode.EXPECTED_JSON_BODY, "Expected json body");
            Application application = JsonSerializer.Deserialize<Application>(bodyString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip
            });
            if (application == null) throw new ApplicationException(ApplicationException.ErrorCode.INVALID_APPLICATION, "Request body is required");
            if (application.company_vat_number == null)
                throw new ApplicationException(ApplicationException.ErrorCode.INVALID_APPLICATION, "company_vat_number is required");
            if (application.given_name == null)
                throw new ApplicationException(ApplicationException.ErrorCode.INVALID_APPLICATION, "given_name is required");
            if (application.family_name == null)
                throw new ApplicationException(ApplicationException.ErrorCode.INVALID_APPLICATION, "family_name is required");
            if (application.email == null)
                throw new ApplicationException(ApplicationException.ErrorCode.INVALID_APPLICATION, "email is required");
            if (application.tax_code == null)
                throw new ApplicationException(ApplicationException.ErrorCode.INVALID_APPLICATION, "tax_code is required");
            if (application.company_category == null)
                throw new ApplicationException(ApplicationException.ErrorCode.INVALID_APPLICATION, "company_category is required");
            if (application.company_category != "WA" && application.company_category != "FA")
                throw new ApplicationException(ApplicationException.ErrorCode.UNKNOW_COMPANY_INDUSTRY_SECTOR, "Unknow company industry sector");
            string bearer_token = headers["Authorization"];
            string id_token = default(string);
            bool isBearerToken = Authentication.TryParseBearerToken(bearer_token, out id_token);
            if (!isBearerToken) throw new AuthenticationException(AuthenticationException.ErrorCode.INVALID_TOKEN, "Bearer token required");
            if (id_token == null) throw new AuthenticationException(AuthenticationException.ErrorCode.CREDENTIALS_REQUIRED, "Credentials required");
            Token token = Authentication.ParseToken(id_token, remoteJwksHub, dateTimeProvider);
            string providerName = remoteJwksHub.GetIssuerName(token.iss);
            using (DbConnection connection = dataSource.OpenConnection())
            {
                UploadApplicationInTransaction(connection, application, providerName, token.sub).Wait();
            }
        }
        catch (AuthenticationException)
        {
            throw;
        }
        catch (ApplicationException)
        {
            throw;
        }
        catch (DbException e)
        {
            throw new Exception("Database error", e);
        }
        catch (JsonException e)
        {
            throw new ApplicationException(ApplicationException.ErrorCode.EXPECTED_JSON_BODY, "Expected json body", e);
        }
        catch (Exception e)
        {
            throw new Exception("Generic error", e);
        }
        return Task.CompletedTask;
    }

    public static Task<string> GetMethod_MyApplication(
        IHeaderDictionary headers,
        DbDataSource dataSource,
        IDateTimeProvider dateTimeProvider,
        IRemoteJwksHub remoteJwksHub
    )
    {
        try
        {
            string id_token = headers["Authorization"];
            if (id_token == null) throw new AuthenticationException(AuthenticationException.ErrorCode.CREDENTIALS_REQUIRED, "Credentials required");
            Token token = Authentication.ParseToken(id_token, remoteJwksHub, dateTimeProvider);
            DbConnection connection = dataSource.OpenConnection();
            DbCommand command = connection.CreateCommand();
            command.CommandText = @"
                SELECT user_id, company_vat_number, given_name, family_name, email, tax_code, company_category
                FROM presentation_letter
                WHERE account_id = (SELECT id FROM user_account WHERE sub=$1 and registered_provider=$2)
            ";
            command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, token.sub));
            string providerName = remoteJwksHub.GetIssuerName(token.iss);
            command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, providerName));
            command.Prepare();
            using (DbDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    Application application = new Application();
                    application.id = reader.GetString(0);
                    application.company_vat_number = reader.GetString(1);
                    application.given_name = reader.GetString(2);
                    application.family_name = reader.GetString(3);
                    application.email = reader.GetString(4);
                    application.tax_code = reader.GetString(5);
                    application.company_category = reader.GetString(6);
                    return Task.FromResult(JsonSerializer.Serialize(application));
                }
            }
            throw new ApplicationException(ApplicationException.ErrorCode.APPLICATION_NOT_FOUND, "Application not found");
        }
        catch (AuthenticationException)
        {
            throw;
        }
        catch (DbException e)
        {
            throw new Exception("Database error", e);
        }
        catch (Exception e)
        {
            throw new Exception("Generic error", e);
        }
    }

    public static Task GetMethod_Applications()
    {
        return Task.CompletedTask;
    }

    public static Task PostMethod_ManageApplication()
    {
        return Task.CompletedTask;
    }
}

public class ApplicationException : Exception
{
    public enum ErrorCode
    {
        GENERIC_ERROR = 0,
        CREDENTIALS_REQUIRED = 1,
        INVALID_TOKEN = 2,
        INVALID_SIGNATURE = 3,
        TOKEN_EXPIRED = 4,
        INVALID_APPLICATION = 5,
        APPLICATION_NOT_FOUND = 6,
        APPLICATION_ALREADY_EXISTS = 7,
        UNKNOW_COMPANY_INDUSTRY_SECTOR = 8,
        COMPANY_INDUSTRY_SECTOR_CONFLICT = 9,
        EXPECTED_JSON_BODY = 10
    }
    public ErrorCode Code { get; } = default(ErrorCode);
    public ApplicationException(ErrorCode errorCode, string message) : base(message)
    {
        this.Code = errorCode;
    }
    public ApplicationException(ErrorCode errorCode, string message, Exception innerException) : base(message, innerException)
    {
        this.Code = errorCode;
    }
}