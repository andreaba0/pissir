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

public class WaterLimit {

    public class PostData {
        public string vat_number { get; set; }
        public float limit { get; set; }
        public string start_date { get; set; }
        public string end_date { get; set; }

        public bool validate() {
                if (vat_number == null || vat_number == string.Empty) {
                    throw new WaterLimitException(WaterLimitException.ErrorCode.INVALID_BODY, "Vat number required");
                }
                if (limit <= 0) {
                    throw new WaterLimitException(WaterLimitException.ErrorCode.INVALID_BODY, "Limit must be greater than 0");
                }
                if (start_date == null || start_date == string.Empty) {
                    throw new WaterLimitException(WaterLimitException.ErrorCode.INVALID_BODY, "Start date required");
                }
                if (end_date == null || end_date == string.Empty) {
                    throw new WaterLimitException(WaterLimitException.ErrorCode.INVALID_BODY, "End date required");
                }
                return true;
            }
    }

    public static Task Post(
        IHeaderDictionary headers,
        Stream body,
        DbDataSource dataSource,
        IDateTimeProvider dateTimeProvider,
        RemoteManager remoteManager
    ) {
        User user = Authorization.AllowByRole(headers, remoteManager, dateTimeProvider, new List<User.Role> { User.Role.WA });

        PostData postData = JsonSerializer.Deserialize<PostData>(body, new JsonSerializerOptions {
            PropertyNameCaseInsensitive = true,
            IncludeFields = true
        });

        DateTime start = dateTimeProvider.parseDateFromFrontend(postData.start_date);
        DateTime end = dateTimeProvider.parseDateFromFrontend(postData.end_date);

        int days = (int)(end - start).TotalDays;

        if(days < 0)
        {
            throw new WaterLimitException(WaterLimitException.ErrorCode.INVALID_BODY, "End date must be greater than start date");
        }

        using DbConnection connection = dataSource.OpenConnection();

        using DbCommand commandPostWaterBody = dataSource.CreateCommand();

        commandPostWaterBody.CommandText = $@"
            insert into daily_water_limit (vat_number, on_date, consumption_sign, available, consumed)
            select $1 as vat_number, date_trunc('day', $2) + (days || ' days')::interval as on_date, 1 as consumption_sign, $3 as available, 0 as consumed
            from generate_series(0, $4) as list(days)
            on conflict (vat_number, on_date) do update set available = $3 + excluded.available
        ";
        commandPostWaterBody.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, postData.vat_number));
        commandPostWaterBody.Parameters.Add(DbUtility.CreateParameter(connection, DbType.DateTime, start));
        commandPostWaterBody.Parameters.Add(DbUtility.CreateParameter(connection, DbType.Single, postData.limit));
        commandPostWaterBody.Parameters.Add(DbUtility.CreateParameter(connection, DbType.Int32, days));

        commandPostWaterBody.ExecuteNonQuery();
    }

    public static ValueTask<string> Get(
        IHeaderDictionary headers,
        DbDataSource dataSource,
        IDateTimeProvider dateTimeProvider,
        RemoteManager remoteManager
    ) {
        User user = Authorization.AllowByRole(headers, remoteManager, dateTimeProvider, new List<User.Role> { User.Role.FA });

        using DbConnection connection = dataSource.OpenConnection();

        using DbCommand commandGetWaterLimit = dataSource.CreateCommand();

        commandGetWaterLimit.CommandText = $@"
            select available+consumed as limit
            from daily_water_limit
            where vat_number = $1 and date_trunc('day', on_date) = date_trunc('day', $2)
        ";
        commandGetWaterLimit.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, user.company_vat_number));
        commandGetWaterLimit.Parameters.Add(DbUtility.CreateParameter(connection, DbType.DateTime, dateTimeProvider.UtcNow));

        using DbDataReader reader1 = commandGetWaterLimit.ExecuteReader();
        if (reader1.HasRows)
        {
            reader1.Read();
            float limit_set = reader1.GetFloat(0);
            connection.Close();
            return new ValueTask<string>(limit_set.ToString());
        }
        reader1.Close();

        commandGetWaterLimit.CommandText = $@"
            select sum(qty) as limit
            from buy_order inner join offer on buy_order.offer_id = offer.id
            inner join farm_field on buy_order.farm_field_id = farm_field.id
            where vat_number = $1 and date_trunc('day', publish_date) = date_trunc('day', $2)
        ";

        using DbDataReader reader2 = commandGetWaterLimit.ExecuteReader();
        if (reader2.HasRows)
        {
            reader2.Read();
            float limit_set = reader2.GetFloat(0);
            connection.Close();
            return new ValueTask<string>(limit_set.ToString());
        }
        reader2.Close();
        connection.Close();
        return new ValueTask<string>("0");
    }
}

public class WaterLimitException : Exception {
    public enum ErrorCode {
        INVALID_AUTHORIZATION_HEADER,
        INVALID_BODY
    }

    public ErrorCode code { get; private set; }
    public WaterLimitException(ErrorCode code, string message) : base(message) {
        this.code = code;
    }
}