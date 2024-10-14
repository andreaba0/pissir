using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data;
using System.Net.Http;
using System.Net.Http.Headers;
using Types;
using Utility;
using Middleware;
using Module.KeyManager;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text;

namespace Routes;

public class ResourceManagerField {
    public struct GetData {
        public string id;
        public float square_meters;
        public string crop_type;
        public string irrigation_type;
    }
    public static string Get(
        IHeaderDictionary headers,
        DbDataSource dataSource,
        IDateTimeProvider dateTimeProvider,
        RemoteManager remoteManager
    ) {
        bool foundAH = Authorization.getAuthorizationHeader(headers, out string authorizationHeader);
        if (!foundAH) {
            throw new AuthenticationException(AuthenticationException.ErrorCode.MISSING_AUTHORIZATION_HEADER, "Missing Authorization header");
        }
        bool parsedAH = Authorization.tryParseAuthorizationHeader(authorizationHeader, out Authorization.Scheme _scheme, out string _token, out string error_message);
        if (!parsedAH) {
            throw new AuthorizationException(AuthorizationException.ErrorCode.INVALID_AUTHORIZATION_HEADER, error_message);
        }
        string[] parts = _token.Split(".");
        string vat_number = Utility.Utility.Base64URLDecode(parts[0]);
        string request_info = Utility.Utility.Base64URLDecode(parts[1]);
        string request_signature = Utility.Utility.Base64URLDecode(parts[2]);
        FarmToken farmToken = JsonSerializer.Deserialize<FarmToken>(request_info, new JsonSerializerOptions {
            PropertyNameCaseInsensitive = true,
        });
        long epochNow = DateTimeProvider.epoch(dateTimeProvider.UtcNow);
        if (epochNow > farmToken.epoch+10) {
            throw new AuthenticationException(AuthenticationException.ErrorCode.TOKEN_EXPIRED, "Token expired");
        }
        if (epochNow < farmToken.epoch) {
            throw new AuthenticationException(AuthenticationException.ErrorCode.INVALID_TOKEN, "Token issued in the future");
        }
        using DbConnection connection = dataSource.OpenConnection();
        using DbCommand commandGetSecret = dataSource.CreateCommand();
        commandGetSecret.CommandText = $@"
            select secret_key
            from secret_key
            where vat_number = $1
        ";
        commandGetSecret.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, vat_number));
        using DbDataReader readerSecret = commandGetSecret.ExecuteReader();
        if (!readerSecret.HasRows) {
            throw new ResourceManagerFieldException(ResourceManagerFieldException.ErrorCode.NO_SECRET_KEY, "No secret key found");
        }
        string secret_key = readerSecret.GetString(0);
        readerSecret.Close();

        string signature = Utility.Utility.HmacSha256(secret_key, request_info);
        if (signature != request_signature) {
            throw new ResourceManagerFieldException(ResourceManagerFieldException.ErrorCode.INVALID_SIGNATURE, "Invalid signature");
        }
        using DbCommand commandGetFields = dataSource.CreateCommand();
        commandGetFields.CommandText = $@"
            select id, square_meters, crop_type, irrigation_type
            from farm_field
            where vat_number = $1
        ";
        commandGetFields.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, vat_number));
        using DbDataReader reader = commandGetFields.ExecuteReader();
        List<GetData> data = new List<GetData>();
        if (!reader.HasRows) {
            reader.Close();
            return "[]";
        }
        while (reader.Read()) {
            data.Add(new GetData {
                id = reader.GetString(0),
                square_meters = reader.GetFloat(1),
                crop_type = reader.GetString(2),
                irrigation_type = reader.GetString(3)
            });
        }
        reader.Close();
        connection.Close();
        return JsonSerializer.Serialize(data, new JsonSerializerOptions {
            IncludeFields = true
        });
    }
}

public class ResourceManagerFieldException : Exception {
    public ResourceManagerFieldException(ErrorCode errorCode, string message) : base(message) {
        this.errorCode = errorCode;
    }
    public ErrorCode errorCode { get; private set; }
    public enum ErrorCode {
        NO_SECRET_KEY = 0,
        INVALID_SIGNATURE = 1
    }
}
        