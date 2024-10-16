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
        FarmToken farmToken = Authorization.AuthorizedPayload(
            headers,
            dateTimeProvider,
            dataSource
        );
        string vat_number = farmToken.sub;
        using DbConnection connection = dataSource.OpenConnection();
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
        