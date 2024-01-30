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
        public DateTime? from { get; set; } = null;
        public DateTime? to { get; set; } = null;
    }

    public static Task PostMethod_ACLRequest(
        IHeaderDictionary headers,
        Stream body,
        DbDataSource dataSource,
        DateTimeProvider dateTimeProvider,
        IRemoteJwksHub remoteJwksHub
    ) {
        try {
            string bearer_token = headers["Authorization"].Count > 0 ? headers["Authorization"].ToString() : string.Empty;
            string id_token = Authentication.ParseBearerToken(bearer_token);
            Token token = Authentication.VerifiedPayload(id_token, remoteJwksHub, dateTimeProvider);

            User user = AuthorizationService.GetUser(remoteJwksHub, dataSource, token.sub, remoteJwksHub.GetIssuerName(token.iss)).Result;

            AccessRequestBody accessRequestBody = JsonSerializer.Deserialize<AccessRequestBody>(body) ?? throw new ApiAccessException(ApiAccessException.ErrorCode.INVALID_REQUEST_BODY, "Invalid request body");

            if(accessRequestBody.from == null) throw new ApiAccessException(ApiAccessException.ErrorCode.INVALID_DATE, "from is required");
            if(accessRequestBody.to == null) throw new ApiAccessException(ApiAccessException.ErrorCode.INVALID_DATE, "to is required");
            if(accessRequestBody.from > accessRequestBody.to) throw new ApiAccessException(ApiAccessException.ErrorCode.INVALID_DATE_RANGE, "from must be less than to");



            using DbConnection connection = dataSource.OpenConnection();

            if(user.role != "FA" && user.role != "WA") throw new ProfileException(ProfileException.ErrorCode.UNKNOW_USER_ROLE, "Unknow user role");

            //check that from - to is less than 3 months
            if(accessRequestBody.to - accessRequestBody.from > TimeSpan.FromDays(90)) throw new ApiAccessException(ApiAccessException.ErrorCode.INVALID_DATE_RANGE, "date range is too large, max 3 months");

            DbCommand command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO api_access_request (user_id, from_date, to_date)
                VALUES ($1, $2, $3)
            ";

            command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, user.global_id));
            command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.DateTime, accessRequestBody.from));
            command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.DateTime, accessRequestBody.to));

            command.ExecuteNonQuery();

            return Task.CompletedTask;
        } catch(Exception) {
            throw;
        }
    }

    public static Task GetMethod_ACLRequest() {
        return Task.FromResult(default(string));
    }

    public static Task PostMethod_ACLAction(
        IHeaderDictionary headers,
        Stream body,
        DbDataSource dataSource,
        DateTimeProvider dateTimeProvider,
        IRemoteJwksHub remoteJwksHub
    ) {
        try {
            string bearer_token = headers["Authorization"].Count > 0 ? headers["Authorization"].ToString() : string.Empty;
            string id_token = Authentication.ParseBearerToken(bearer_token);
            Token token = Authentication.VerifiedPayload(id_token, remoteJwksHub, dateTimeProvider);

            User user = AuthorizationService.GetUser(remoteJwksHub, dataSource, token.sub, remoteJwksHub.GetIssuerName(token.iss)).Result;

            AccessRequestBody accessRequestBody = JsonSerializer.Deserialize<AccessRequestBody>(body) ?? throw new ApiAccessException(ApiAccessException.ErrorCode.INVALID_REQUEST_BODY, "Invalid request body");

            if(accessRequestBody.from == null) throw new ApiAccessException(ApiAccessException.ErrorCode.INVALID_DATE, "from is required");
            if(accessRequestBody.to == null) throw new ApiAccessException(ApiAccessException.ErrorCode.INVALID_DATE, "to is required");
            if(accessRequestBody.from > accessRequestBody.to) throw new ApiAccessException(ApiAccessException.ErrorCode.INVALID_DATE_RANGE, "from must be less than to");
        
            return Task.CompletedTask;
        }
        catch(Exception) {
            throw;
        }
    }

    internal static Task PostMethod_ACLAccept(
        DbConnection connection,
        AccessRequestBody accessRequestBody,
        DbDataSource dataSource,
        string userId,
        string aclId
    ) {
        try {
            DbCommand transactionInit = connection.CreateCommand();
            transactionInit.CommandText = "BEGIN";
            transactionInit.ExecuteNonQuery();

            //check if to and from are != null
            if(accessRequestBody.from == null) throw new ApiAccessException(ApiAccessException.ErrorCode.INVALID_DATE, "from is required");
            if(accessRequestBody.to == null) throw new ApiAccessException(ApiAccessException.ErrorCode.INVALID_DATE, "to is required");
            int numberOfDays = (int)(accessRequestBody.to - accessRequestBody.from).Value.TotalDays;
            for(int i=0;i<numberOfDays;i+=10) {
                int step = (numberOfDays - i > 10) ? 10 : numberOfDays - i;
                DbCommand command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO api_acl (user_id, date_allowed)
                    $1, select date(generate_series($2, $3, interval '1 day'))
                    ON CONFLICT DO NOTHING
                ";
                command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, userId));
                command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.DateTime, accessRequestBody.from.Value.AddDays(i)));
                command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.DateTime, accessRequestBody.from.Value.AddDays(i+step)));
                command.ExecuteNonQuery();
            }

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

    internal Task PostMethod_ACLReject(
        DbConnection connection,
        AccessRequestBody accessRequestBody,
        DbDataSource dataSource,
        string userId,
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
}

public class ApiAccessException : Exception {
    public enum ErrorCode {
        GENERIC_ERROR = 0,
        INVALID_DATE = 1,
        INVALID_COMPANY_VAT_NUMBER = 2,
        INVALID_DATE_RANGE = 3,
        INVALID_REQUEST_BODY = 4
    }
    public ErrorCode Code { get; } = default(ErrorCode);
    public ApiAccessException(ErrorCode errorCode, string message) : base(message) {
        Code = errorCode;
    }
    public ApiAccessException(ErrorCode errorCode, string message, Exception innerException) : base(message, innerException) {
        Code = errorCode;
    }
}