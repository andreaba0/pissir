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
            string companyVatNumber = application.company_vat_number ?? throw new ApplicationException(ApplicationException.ErrorCode.INVALID_APPLICATION, "company_vat_number is required");
            command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, companyVatNumber));
            string givenName = application.given_name ?? throw new ApplicationException(ApplicationException.ErrorCode.INVALID_APPLICATION, "given_name is required");
            command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, givenName));
            string familyName = application.family_name ?? throw new ApplicationException(ApplicationException.ErrorCode.INVALID_APPLICATION, "family_name is required");
            command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, familyName));
            string email = application.email ?? throw new ApplicationException(ApplicationException.ErrorCode.INVALID_APPLICATION, "email is required");
            command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, email));
            string taxCode = application.tax_code ?? throw new ApplicationException(ApplicationException.ErrorCode.INVALID_APPLICATION, "tax_code is required");
            command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, taxCode));
            string companyIndustrySector = application.company_category ?? throw new ApplicationException(ApplicationException.ErrorCode.INVALID_APPLICATION, "company_category is required");
            command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, companyIndustrySector));
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
            Application application = JsonSerializer.DeserializeAsync<Application>(body, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip
            }).Result ?? throw new ApplicationException(ApplicationException.ErrorCode.EXPECTED_JSON_BODY, "Expected json body");
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
            if (application.company_category != "WSP" && application.company_category != "FAR")
                throw new ApplicationException(ApplicationException.ErrorCode.UNKNOW_COMPANY_INDUSTRY_SECTOR, "Unknow company industry sector");

            string bearer_token = headers["Authorization"].Count > 0 ? headers["Authorization"].ToString() : string.Empty;
            string id_token = Authentication.ParseBearerToken(bearer_token);
            Token token = Authentication.VerifiedPayload(id_token, remoteJwksHub, dateTimeProvider);
            string providerName = remoteJwksHub.GetIssuerName(token.iss);

            if(token.given_name!=string.Empty && token.given_name != application.given_name) 
                throw new ApplicationException(ApplicationException.ErrorCode.BODY_FIELD_DOES_NOT_MATCH_JWT_FIELD, "Body field given_name does not match provider field given_name");
            if(token.family_name!=string.Empty && token.family_name != application.family_name)
                throw new ApplicationException(ApplicationException.ErrorCode.BODY_FIELD_DOES_NOT_MATCH_JWT_FIELD, "Body field family_name does not match provider field family_name");
            if(token.email!=string.Empty && token.email != application.email)
                throw new ApplicationException(ApplicationException.ErrorCode.BODY_FIELD_DOES_NOT_MATCH_JWT_FIELD, "Body field email does not match provider field email");

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
        catch (DbException)
        {
            throw;
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
            string bearer_token = headers["Authorization"].Count > 0 ? headers["Authorization"].ToString() : string.Empty;
            string id_token = Authentication.ParseBearerToken(bearer_token);
            Token token = Authentication.VerifiedPayload(id_token, remoteJwksHub, dateTimeProvider);
            string providerName = remoteJwksHub.GetIssuerName(token.iss);
            using DbConnection connection = dataSource.OpenConnection();
            DbCommand command = connection.CreateCommand();
            command.CommandText = @"
                SELECT 
                    presentation_id, 
                    company_vat_number, 
                    company_industry_sector, 
                    given_name, 
                    family_name, 
                    email, 
                    tax_code 
                FROM 
                    presentation_letter
                WHERE 
                    user_account = (SELECT id FROM user_account WHERE sub=$1 and registered_provider=$2)
            ";
            command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, token.sub));
            command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, providerName));
            command.Prepare();
            Application application = new Application();
            using (DbDataReader reader = command.ExecuteReader())
            {
                if (!reader.HasRows) throw new ApplicationException(ApplicationException.ErrorCode.APPLICATION_NOT_FOUND, "Application not found");
                if (reader.Read())
                {
                    application.id = reader.GetGuid(0).ToString();
                    application.company_vat_number = reader.GetString(1);
                    application.company_category = reader.GetString(2);
                    application.given_name = reader.GetString(3);
                    application.family_name = reader.GetString(4);
                    application.email = reader.GetString(5);
                    application.tax_code = reader.GetString(6);
                }
            }
            connection.Close();
            return Task.FromResult(JsonSerializer.Serialize(application));
        }
        catch (AuthenticationException)
        {
            throw;
        }
        catch (DbException e)
        {
            throw new Exception("Database error", e);
        }
        catch (ApplicationException)
        {
            throw;
        }
        catch (Exception e)
        {
            throw new Exception("Generic error", e);
        }
    }

    public static Task<string> GetMethod_Applications(
        IHeaderDictionary headers,
        IQueryCollection query,
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

            //check if user is working for a WA company, otherwise return unauthorized
            DbCommand companyCommand = connection.CreateCommand();
            companyCommand.CommandText = @"
                SELECT
                    person_role
                FROM
                    person inner join user_account on person.account_id=user_account.id
                WHERE
                    user_account.sub=$1 and user_account.registered_provider=$2
            ";
            companyCommand.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, token.sub));
            companyCommand.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, providerName));
            companyCommand.Prepare();
            using (DbDataReader reader = companyCommand.ExecuteReader())
            {
                if (!reader.HasRows) throw new ApplicationException(ApplicationException.ErrorCode.USER_ACCOUNT_NOT_FOUND, "User account not found");
                if (reader.Read())
                {
                    string personRole = reader.GetString(0);
                    if (personRole != "WA") throw new AuthenticationException(AuthenticationException.ErrorCode.USER_UNAUTHORIZED, "User unauthorized");
                }
            }


            DbCommand command = connection.CreateCommand();
            command.CommandText = @"
                SELECT 
                    presentation_id, 
                    company_vat_number, 
                    company_industry_sector, 
                    given_name, 
                    family_name, 
                    email, 
                    tax_code 
                FROM 
                    presentation_letter
                ORDER BY 
                    created_at ASC
                LIMIT $3
                OFFSET $4 
                
            ";
            command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, token.sub));
            command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, providerName));
            string limit = query["count_per_page"].Count > 0 ? query["count_per_page"].ToString() : "10";
            string page_number = query["page_number"].Count > 0 ? query["page_number"].ToString() : "0";
            int offset = int.Parse(limit) * (int.Parse(page_number)-1);
            if (offset < 0) offset = 0;
            command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.Int32, int.Parse(limit)));
            command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.Int32, offset));
            command.Prepare();
            List<Application> applications = new List<Application>();
            using (DbDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    Application application = new Application();
                    application.id = reader.GetGuid(0).ToString();
                    application.company_vat_number = reader.GetString(1);
                    application.company_category = reader.GetString(2);
                    application.given_name = reader.GetString(3);
                    application.family_name = reader.GetString(4);
                    application.email = reader.GetString(5);
                    application.tax_code = reader.GetString(6);
                    applications.Add(application);
                }
            }
            connection.Close();
            return Task.FromResult(JsonSerializer.Serialize(applications));
        }
        catch (AuthenticationException)
        {
            throw;
        }
        catch (DbException e)
        {
            throw new Exception("Database error", e);
        }
        catch (ApplicationException)
        {
            throw;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception("Generic error", e);
        }
    }

    public static Task PostMethod_ManageApplication(
        IHeaderDictionary headers,
        RouteValueDictionary query,
        DbDataSource dataSource,
        IDateTimeProvider dateTimeProvider,
        IRemoteJwksHub remoteJwksHub
    )
    {
        try
        {
            string applicationId = query?["id"]?.ToString() ?? string.Empty;
            //check if applicationId is a uuid
            if (!Guid.TryParse(applicationId, out Guid _)) throw new ApplicationException(ApplicationException.ErrorCode.INVALID_APPLICATION, "Invalid application id");
            if (applicationId == string.Empty) throw new ApplicationException(ApplicationException.ErrorCode.INVALID_APPLICATION, "Application id is required");
            string action = query?["action"]?.ToString() ?? string.Empty;
            if (action == string.Empty) throw new ApplicationException(ApplicationException.ErrorCode.INVALID_APPLICATION, "Action is required");
            if (action != "accept" && action != "reject") throw new ApplicationException(ApplicationException.ErrorCode.INVALID_APPLICATION, "Invalid action");
            string bearer_token = headers["Authorization"].Count > 0 ? headers["Authorization"].ToString() : string.Empty;
            string id_token = Authentication.ParseBearerToken(bearer_token);
            Token token = Authentication.VerifiedPayload(id_token, remoteJwksHub, dateTimeProvider);
            string providerName = remoteJwksHub.GetIssuerName(token.iss);
            using (DbConnection connection = dataSource.OpenConnection())
            {
                //check if user is working for a WA company, otherwise return unauthorized
                DbCommand companyCommand = connection.CreateCommand();
                companyCommand.CommandText = @"
                    SELECT
                        person_role
                    FROM
                        person inner join user_account 
                        on 
                            person.account_id=user_account.id
                    WHERE
                        user_account.sub=$1 and user_account.registered_provider=$2
                ";
                companyCommand.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, token.sub));
                companyCommand.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, providerName));
                companyCommand.Prepare();
                using (DbDataReader reader = companyCommand.ExecuteReader())
                {
                    if (!reader.HasRows) throw new ApplicationException(ApplicationException.ErrorCode.USER_ACCOUNT_NOT_FOUND, "User account not found");
                    if (reader.Read())
                    {
                        string personRole = reader.GetString(0);
                        if (personRole != "WA") throw new AuthenticationException(AuthenticationException.ErrorCode.USER_UNAUTHORIZED, "User unauthorized");
                    }
                }

                if (action == "accept")
                {
                    ManageApplication_TransactionApprove(
                        token,
                        connection,
                        providerName,
                        applicationId,
                        Application.ApplicationAction.APPROVE
                    ).Wait();
                }
                else
                {
                    ManageApplication_Reject(
                        token,
                        connection,
                        providerName,
                        applicationId,
                        Application.ApplicationAction.REJECT
                    ).Wait();
                }
            }
            return Task.CompletedTask;
        }
        catch (AuthenticationException)
        {
            throw;
        }
        catch (DbException e)
        {
            throw new Exception("Database error", e);
        }
        catch (ApplicationException)
        {
            throw;
        }
        catch (Exception e)
        {
            throw new Exception("Generic error", e);
        }
    }

    internal enum ApplicationAction
    {
        APPROVE,
        REJECT
    }

    internal static Task ManageApplication_Reject(
        Token token,
        DbConnection connection,
        string providerName,
        string applicationId,
        ApplicationAction action
    )
    {
        try
        {
            //begin transaction using plain sql command
            DbCommand transactionCommand = connection.CreateCommand();

            //delete application id from presentation_letter table and return its data
            DbCommand command = connection.CreateCommand();
            command.CommandText = @"
                DELETE FROM 
                    presentation_letter
                WHERE 
                    presentation_id=$1
                RETURNING 
                    user_account
            ";
            command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.Guid, Guid.Parse(applicationId)));
            command.Prepare();
            Application application = new Application();
            using (DbDataReader reader = command.ExecuteReader())
            {
                if (!reader.HasRows) throw new ApplicationException(ApplicationException.ErrorCode.APPLICATION_NOT_FOUND, "Application not found");
            }
            return Task.CompletedTask;
        }
        catch (DbException)
        {
            throw;
        }
        catch (Exception)
        {
            throw;
        }
    }

    internal static Task ManageApplication_TransactionApprove(
        Token token,
        DbConnection connection,
        string providerName,
        string applicationId,
        ApplicationAction action
    )
    {
        try
        {
            //begin transaction using plain sql command
            DbCommand transactionCommand = connection.CreateCommand();
            transactionCommand.CommandText = "BEGIN TRANSACTION ISOLATION LEVEL READ COMMITTED";
            transactionCommand.ExecuteNonQuery();

            //delete application id from presentation_letter table and return its data
            DbCommand command = connection.CreateCommand();
            command.CommandText = @"
                DELETE FROM 
                    presentation_letter
                WHERE 
                    presentation_id=$1
                RETURNING 
                    company_vat_number, 
                    given_name, 
                    family_name, 
                    email, 
                    tax_code, 
                    company_industry_sector,
                    user_account
            ";
            command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.Guid, Guid.Parse(applicationId)));
            command.Prepare();
            Application application = new Application();
            string userAccount = string.Empty;
            using (DbDataReader reader = command.ExecuteReader())
            {
                if (!reader.HasRows) throw new ApplicationException(ApplicationException.ErrorCode.APPLICATION_NOT_FOUND, "Application not found");
                if (reader.Read())
                {
                    application.company_vat_number = reader.GetString(0);
                    application.given_name = reader.GetString(1);
                    application.family_name = reader.GetString(2);
                    application.email = reader.GetString(3);
                    application.tax_code = reader.GetString(4);
                    application.company_category = reader.GetString(5);
                    userAccount = reader.GetInt64(6).ToString();
                }
            }

            //insert into company table
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
            string companyVatNumber = application.company_vat_number ?? throw new ApplicationException(ApplicationException.ErrorCode.INVALID_APPLICATION, "company_vat_number is required");
            string companyIndustrySector = application.company_category ?? throw new ApplicationException(ApplicationException.ErrorCode.INVALID_APPLICATION, "company_industry_sector is required");
            companyCommand.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, companyVatNumber));
            companyCommand.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, companyIndustrySector));
            companyCommand.Prepare();
            companyCommand.ExecuteNonQuery();

            //insert into respective company specialization
            DbCommand specializationCommand = connection.CreateCommand();
            string specializationTable = (application.company_category == "WSP") ? "company_wsp" : "company_far";
            specializationCommand.CommandText = @"
                INSERT INTO 
                    " + specializationTable + @" (
                        vat_number,
                        industry_sector
                    )
                VALUES (
                    $1,
                    $2
                )
                ON CONFLICT (vat_number) DO NOTHING
            ";
            specializationCommand.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, application.company_vat_number));
            specializationCommand.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, application.company_category));
            specializationCommand.Prepare();
            specializationCommand.ExecuteNonQuery();

            //insert into person table
            DbCommand personCommand = connection.CreateCommand();
            personCommand.CommandText = @"
                INSERT INTO 
                    person (
                        given_name,
                        family_name,
                        email,
                        tax_code,
                        account_id,
                        person_role
                    )
                VALUES (
                    $1,
                    $2,
                    $3,
                    $4,
                    $5,
                    $6
                )
            ";
            personCommand.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, application.given_name));
            personCommand.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, application.family_name));
            personCommand.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, application.email));
            personCommand.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, application.tax_code));
            personCommand.Parameters.Add(DbUtility.CreateParameter(connection, DbType.Int64, long.Parse(userAccount)));
            if (application.company_category == "WSP")
            {
                personCommand.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, "WA"));
            }
            else
            {
                personCommand.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, "FA"));
            }
            personCommand.Prepare();
            personCommand.ExecuteNonQuery();

            //insert user into specialized table
            DbCommand userCommand = connection.CreateCommand();
            string userTable = (application.company_category == "WSP") ? "person_wa" : "person_fa";
            userCommand.CommandText = @"
                INSERT INTO 
                    " + userTable + @" (
                        account_id,
                        role_name,
                        company_vat_number
                    )
                VALUES (
                    $1,
                    $2,
                    $3
                )
            ";
            userCommand.Parameters.Add(DbUtility.CreateParameter(connection, DbType.Int64, long.Parse(userAccount)));
            if (application.company_category == "WSP")
            {
                userCommand.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, "WA"));
            }
            else
            {
                userCommand.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, "FA"));
            }
            userCommand.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, application.company_vat_number));
            userCommand.Prepare();
            userCommand.ExecuteNonQuery();

            //commit transaction
            DbCommand commitCommand = connection.CreateCommand();
            commitCommand.CommandText = "COMMIT";
            commitCommand.ExecuteNonQuery();
            connection.Close();
            return Task.CompletedTask;
        }
        catch (DbException e) when (e.InnerException is not IOException)
        {
            Console.WriteLine(e);
            DbCommand rollbackCommand = connection.CreateCommand();
            rollbackCommand.CommandText = "ROLLBACK";
            rollbackCommand.ExecuteNonQuery();
            throw;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
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
        EXPECTED_JSON_BODY = 10,
        USER_ACCOUNT_NOT_FOUND = 11,
        INSUFFICIENT_PERMISSIONS = 12,
        USER_ALREADY_EXISTS = 13,
        BODY_FIELD_DOES_NOT_MATCH_JWT_FIELD = 14,
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