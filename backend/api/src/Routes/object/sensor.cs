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

namespace Routes;

public class SensorRoute {
    public struct GetData {
        public string object_id;
        public string field_id;
        public string type;
        public DateTime time;
        public float value;
    }

    public static List<GetData> GetSensorData(
        IHeaderDictionary headers,
        DbDataSource dataSource,
        IDateTimeProvider dateTimeProvider,
        RemoteManager remoteManager
    ) {
        bool ok = false;
        string bearer_token = headers["Authorization"].Count > 0 ? headers["Authorization"].ToString() : string.Empty;
        ok = Authorization.tryParseAuthorizationHeader(bearer_token, out Authorization.Scheme _scheme, out string _token, out string error_message);
        if(!ok) {
            throw new AuthorizationException(AuthorizationException.ErrorCode.INVALID_AUTHORIZATION_HEADER, "Invalid authorization header");
        }
        if (_scheme != Authorization.Scheme.Bearer) {
            throw new AuthorizationException(AuthorizationException.ErrorCode.INVALID_AUTHORIZATION_HEADER, "Bearer scheme required");
        }
        Token token = Authentication.VerifiedPayload(_token, remoteManager, dateTimeProvider);
        User user = new User(
            global_id: token.sub,
            role: token.role,
            company_vat_number: token.company_vat_number
        );

        if (User.GetRole(user) != User.Role.FA) {
            throw new AuthorizationException(AuthorizationException.ErrorCode.INVALID_ROLE, "Only FA users can access this route");
        }

        using DbConnection connection = dataSource.OpenConnection();

        using DbCommand commandGetSensorData = dataSource.CreateCommand();

        commandGetSensorData.CommandText = $@"
            select object_id, log_time as time, farm_field_id as field_id, value, object_type as type
            from combined_sensor_data inner join object_logger on
                combined_sensor_data.object_id = object_logger.id and combined_sensor_data.object_type = object_logger.object_type
                inner join farm_field on
                object_logger.farm_field_id = farm_field.id
            where farm_field.vat_number = $1
        ";

        commandGetSensorData.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, user.company_vat_number));

        using DbDataReader reader = commandGetSensorData.ExecuteReader();

        if (!reader.HasRows) {
            return new List<GetData>();
        }

        List<GetData> data = new List<GetData>();

        while (reader.Read()) {
            data.Add(new GetData {
                object_id = reader.GetString(0),
                time = reader.GetDateTime(1),
                field_id = reader.GetString(2),
                value = reader.GetFloat(3),
                type = reader.GetString(4)
            });
        }
        reader.Close();
        connection.Close();
        return data;
    }
}