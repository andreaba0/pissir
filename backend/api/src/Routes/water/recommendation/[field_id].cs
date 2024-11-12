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
using System.Security.Cryptography;
using System.Globalization;

namespace Routes;

public class WaterRecommendationFieldId {
    public struct GetData {
        public float total_estimated;
        public float total_remaining;
    }
    public static ValueTask<string> Get(
        IHeaderDictionary headers,
        RouteValueDictionary routeValues,
        DbDataSource dbDataSource,
        IDateTimeProvider dateTimeProvider,
        RemoteManager remoteManager
    ) {
        User user = Authorization.AllowByRole(headers, remoteManager, dateTimeProvider, new List<User.Role> { User.Role.FA });

        string fieldId = routeValues["field_id"].ToString();
        if (fieldId == string.Empty) {
            throw new WaterRecommendationFieldIdException(WaterRecommendationFieldIdException.ErrorCode.FIELD_ID_REQUIRED, "Field id required");
        }

        using DbConnection connection = dbDataSource.OpenConnection();
        DbCommand command = dbDataSource.CreateCommand();
        command.CommandText = $@"
            select fv.square_meters*cf.liters_mq as total_estimated
            from farm_field as field inner join
            farm_field_versioning as fv on field.id = fv.field_id inner join
            consumption_fact as cf on fv.crop_type = cf.crop
            where field.id = $1 and field.vat_number = $2
            order by fv.created_at desc
            limit 1
        ";
        command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, fieldId));
        command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, user.company_vat_number));
        float totalEstimated = 0;
        float totalWaterUsed = 0;
        float totalWaterBought = 0;
        //using DbDataReader reader = command.ExecuteReader();
        using (DbDataReader reader = command.ExecuteReader()) {
            if (!reader.HasRows) {
                throw new WaterRecommendationFieldIdException(WaterRecommendationFieldIdException.ErrorCode.NOT_FOUND, "Field not found");
            }

            reader.Read();
            totalEstimated = reader.GetFloat(0);
            reader.Close();
        }

        command.Parameters.Clear();

        // Union is required to handle edge case where logs for current day do not exist
        // because actuator may have been turned on since the past day
        command.CommandText = $@"
            select sum(water_used)
            from actuator_log as al inner join
            object_logger as ol on al.object_id = ol.id inner join
            farm_field as field on ol.farm_field_id = field.id
            where ol.farm_field_id = $1 and field.vat_number = $3 and
            date_trunc('day', al.log_time) = date_trunc('day', $2)
            limit 1
        ";

        command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, fieldId));
        command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.DateTime, dateTimeProvider.UtcNow));
        command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, user.company_vat_number));

        using (DbDataReader reader = command.ExecuteReader()) {
            if (!reader.HasRows) {
                throw new WaterRecommendationFieldIdException(WaterRecommendationFieldIdException.ErrorCode.NOT_FOUND, "Field not found");
            }

            reader.Read();
            totalWaterUsed = reader.GetFloat(0);
            reader.Close();
        }

        command.Parameters.Clear();

        command.CommandText = $@"
            select sum(qty)
            from buy_order as bo inner join
            offer as o on bo.offer_id = o.id inner join
            farm_field as field on bo.farm_field_id = field.id
            where field.id = $1 and field.vat_number = $2 and date_trunc('day', o.publish_date) = date_trunc('day', $3)
            limit 1
        ";
        command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, fieldId));
        command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, user.company_vat_number));
        command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.DateTime, dateTimeProvider.UtcNow));

        using (DbDataReader reader = command.ExecuteReader()) {
            if (!reader.HasRows) {
                throw new WaterRecommendationFieldIdException(WaterRecommendationFieldIdException.ErrorCode.NOT_FOUND, "Field not found");
            }

            reader.Read();
            totalWaterBought = reader.GetFloat(0);
            reader.Close();
        }


        string res = JsonSerializer.Serialize(new GetData {
            total_estimated = totalEstimated,
            total_remaining = totalWaterBought - totalWaterUsed
        }, new JsonSerializerOptions {
            IncludeFields = true
        });
        return new ValueTask<string>(res);
    }
}

public class WaterRecommendationFieldIdException : Exception {
    public enum ErrorCode {
        FIELD_ID_REQUIRED,
        NOT_FOUND
    }
    public ErrorCode Code { get; }
    public WaterRecommendationFieldIdException(ErrorCode code, string message) : base(message) {
        Code = code;
    }
    public WaterRecommendationFieldIdException(ErrorCode code) {
        Code = code;
    }
}