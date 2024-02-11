using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Net;
using Module.KeyManager;
using Module.Middleware;
using Module.Openid;
using Utility;
using Types;

using AuthorizationService = Module.Middleware.Authorization;

namespace Routes;

public class ApiAccess {

    internal class AccessRequestBody {
        public DateTime? date_start { get; set; } = null;
        public DateTime? date_end { get; set; } = null;
    }

    public class AccessRequestRow {
        public DateTime? date_start { get; set; } = null;
        public DateTime? date_end { get; set; } = null;
        public string? acl_id { get; set; } = null;
        public string? first_name { get; set; } = null;
        public string? last_name { get; set; } = null;
        public string? company_vat_number { get; set; } = null;
        public string? email { get; set; } = null;
    }

    public static Task PostMethod_ACLRequest(
        IHeaderDictionary headers,
        Stream body,
        DbDataSource dataSource,
        IDateTimeProvider dateTimeProvider,
        IRemoteJwksHub remoteJwksHub
    ) {
        try {
            string bearer_token = headers["Authorization"].Count > 0 ? headers["Authorization"].ToString() : string.Empty;
            string id_token = Authentication.ParseBearerToken(bearer_token);
            Token token = Authentication.VerifiedPayload(id_token, remoteJwksHub, dateTimeProvider);

            User user = AuthorizationService.GetUser(remoteJwksHub, dataSource, token.sub, remoteJwksHub.GetIssuerName(token.iss)).Result;

            AccessRequestBody accessRequestBody = JsonSerializer.DeserializeAsync<AccessRequestBody>(body, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip
            }).Result ?? throw new ApiAccessException(ApiAccessException.ErrorCode.INVALID_REQUEST_BODY, "Invalid request body");

            if(accessRequestBody.date_start == null) throw new ApiAccessException(ApiAccessException.ErrorCode.INVALID_DATE, "start date is required");
            if(accessRequestBody.date_end == null) throw new ApiAccessException(ApiAccessException.ErrorCode.INVALID_DATE, "end date is required");
            if(accessRequestBody.date_start > accessRequestBody.date_end) throw new ApiAccessException(ApiAccessException.ErrorCode.INVALID_DATE_RANGE, "start date must be smaller than end date");



            using DbConnection connection = dataSource.OpenConnection();

            if(user.role != "FA" && user.role != "WA") throw new ProfileException(ProfileException.ErrorCode.UNKNOW_USER_ROLE, "Unknow user role");
            if(user.role == "FA") throw new AuthorizationException(AuthorizationException.ErrorCode.UNAUTHORIZED, "User is not authorized to perform this action");

            DbCommand command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO api_access_request (acl_id, user_id, sdate, edate)
                VALUES ($1, (
                    SELECT id
                    FROM person_fa
                    WHERE global_id=$2
                ), $3, $4)
            ";

            command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.Guid, Guid.NewGuid()));
            command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.Guid, user.global_id));
            command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.DateTime, accessRequestBody.date_start));
            command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.DateTime, accessRequestBody.date_end));

            command.ExecuteNonQuery();

            return Task.CompletedTask;
        } catch(Exception) {
            throw;
        }
    }

    public static Task<List<AccessRequestRow>> GetMethod_ACLRequest(
        IHeaderDictionary headers,
        IQueryCollection query,
        DbDataSource dataSource,
        IDateTimeProvider dateTimeProvider,
        IRemoteJwksHub remoteJwksHub
    ) {
        try {
            string limit = query["count_per_page"].Count > 0 ? query["count_per_page"].ToString() : "10";
            string page_number = query["page_number"].Count > 0 ? query["page_number"].ToString() : "0";
            string bearer_token = headers["Authorization"].Count > 0 ? headers["Authorization"].ToString() : string.Empty;
            string id_token = Authentication.ParseBearerToken(bearer_token);
            Token token = Authentication.VerifiedPayload(id_token, remoteJwksHub, dateTimeProvider);

            User user = AuthorizationService.GetUser(remoteJwksHub, dataSource, token.sub, remoteJwksHub.GetIssuerName(token.iss)).Result;

            if(user.role != "FA" && user.role != "WA") throw new ProfileException(ProfileException.ErrorCode.UNKNOW_USER_ROLE, "Unknow user role");
            if(user.role == "FA") throw new AuthorizationException(AuthorizationException.ErrorCode.UNAUTHORIZED, "User is not authorized to perform this action");

            using DbConnection connection = dataSource.OpenConnection();
            DbCommand command = connection.CreateCommand();
            command.CommandText = @"
                SELECT 
                    acl_id,
                    sdate,
                    edate,
                    p.given_name,
                    p.family_name,
                    pfa.company_vat_number,
                    p.email
                FROM 
                    api_acl_request inner join person as p on api_acl_request.person_fa = p.account_id
                    inner join person_fa as pfa on api_acl_request.person_fa = pfa.account_id
                order by created_at asc
                limit $1 offset $2
            ";
            int offset = int.Parse(limit) * (int.Parse(page_number)-1);
            if(offset < 0) offset = 0;
            command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.Int32, int.Parse(limit)));
            command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.Int32, offset));
            List<AccessRequestRow> accessRequests = new List<AccessRequestRow>();
            using (DbDataReader reader = command.ExecuteReader()) {
                while(reader.Read()) {
                    AccessRequestRow accessRequest = new AccessRequestRow();
                    accessRequest.acl_id = reader.GetGuid(0).ToString();
                    accessRequest.date_start = reader.GetDateTime(1);
                    accessRequest.date_end = reader.GetDateTime(2);
                    accessRequest.first_name = reader.GetString(3);
                    accessRequest.last_name = reader.GetString(4);
                    accessRequest.company_vat_number = reader.GetString(5);
                    accessRequest.email = reader.GetString(6);
                    accessRequests.Add(accessRequest);
                }
            }
            return Task.FromResult(accessRequests);
        } catch(Exception) {
            throw;
        }
    }

    public static Task PostMethod_ACLAction(
        IHeaderDictionary headers,
        RouteValueDictionary query,
        DbDataSource dataSource,
        IDateTimeProvider dateTimeProvider,
        IRemoteJwksHub remoteJwksHub
    ) {
        try {
            string bearer_token = headers["Authorization"].Count > 0 ? headers["Authorization"].ToString() : string.Empty;
            string id_token = Authentication.ParseBearerToken(bearer_token);
            Token token = Authentication.VerifiedPayload(id_token, remoteJwksHub, dateTimeProvider);

            string acl_id = query["id"].ToString() ?? string.Empty;
            string action = query["action"].ToString() ?? string.Empty;

            if(action != "accept" && action != "reject") throw new ApiAccessException(ApiAccessException.ErrorCode.INVALID_ACTION, "Invalid action");

            User user = AuthorizationService.GetUser(remoteJwksHub, dataSource, token.sub, remoteJwksHub.GetIssuerName(token.iss)).Result;

            if(user.role != "FA" && user.role != "WA") throw new ProfileException(ProfileException.ErrorCode.UNKNOW_USER_ROLE, "Unknow user role");
            if(user.role == "FA") throw new AuthorizationException(AuthorizationException.ErrorCode.UNAUTHORIZED, "User is not authorized to perform this action");

            using (DbConnection connection = dataSource.OpenConnection()) {
                if(action=="accept") PostMethod_ACLAccept(
                    connection,
                    acl_id
                ); 
                else if(action=="reject") PostMethod_ACLReject(
                    connection,
                    acl_id
                );
            }
        
            return Task.CompletedTask;
        }
        catch(Exception) {
            throw;
        }
    }

    internal static Task PostMethod_ACLAccept(
        DbConnection connection,
        string aclId
    ) {
        try {
            DbCommand transactionInit = connection.CreateCommand();
            transactionInit.CommandText = "BEGIN";
            transactionInit.ExecuteNonQuery();

            DbCommand deleteCommand = connection.CreateCommand();
            deleteCommand.CommandText = @"
                DELETE FROM api_acl_request
                WHERE acl_id=$1
                RETURNING sdate, edate, person_fa
            ";
            deleteCommand.Parameters.Add(DbUtility.CreateParameter(connection, DbType.Guid, Guid.Parse(aclId)));

            AccessRequestBody accessRequestBody = new AccessRequestBody();
            string? person_fa = string.Empty;
            using (DbDataReader reader = deleteCommand.ExecuteReader()) {
                if(!reader.HasRows) throw new ApiAccessException(ApiAccessException.ErrorCode.GENERIC_ERROR, "ACL not found");
                while(reader.Read()) {
                    accessRequestBody.date_start = reader.GetDateTime(0);
                    accessRequestBody.date_end = reader.GetDateTime(1);
                    person_fa = reader.GetInt64(2).ToString();
                }
            }

            DbCommand insertCommand = connection.CreateCommand();
            insertCommand.CommandText = @"
                INSERT INTO api_acl (person_fa, sdate, edate)
                VALUES ($1, $2, $3)
            ";
            insertCommand.Parameters.Add(DbUtility.CreateParameter(connection, DbType.Int64, long.Parse(person_fa)));
            insertCommand.Parameters.Add(DbUtility.CreateParameter(connection, DbType.DateTime, accessRequestBody.date_start));
            insertCommand.Parameters.Add(DbUtility.CreateParameter(connection, DbType.DateTime, accessRequestBody.date_end));
            insertCommand.ExecuteNonQuery();

            DbCommand transactionEnd = connection.CreateCommand();
            transactionEnd.CommandText = "COMMIT";
            transactionEnd.ExecuteNonQuery();
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

    internal static Task PostMethod_ACLReject(
        DbConnection connection,
        string aclId
    ) {
        try {
            DbCommand deleteCommand = connection.CreateCommand();
            deleteCommand.CommandText = @"
                DELETE FROM api_access_request
                WHERE acl_id=$1
            ";
            deleteCommand.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, aclId));
            deleteCommand.ExecuteNonQuery();
            return Task.CompletedTask;
        }
        catch (Exception)
        {
            throw;
        }
    }

    public static Task PostMethod_Token() {
        return Task.CompletedTask;
    }
}

public class ApiAccessException : Exception {
    public enum ErrorCode {
        GENERIC_ERROR = 0,
        INVALID_DATE = 1,
        INVALID_COMPANY_VAT_NUMBER = 2,
        INVALID_DATE_RANGE = 3,
        INVALID_REQUEST_BODY = 4,
        INVALID_ACTION = 5
    }
    public ErrorCode Code { get; } = default(ErrorCode);
    public ApiAccessException(ErrorCode errorCode, string message) : base(message) {
        Code = errorCode;
    }
    public ApiAccessException(ErrorCode errorCode, string message, Exception innerException) : base(message, innerException) {
        Code = errorCode;
    }
}