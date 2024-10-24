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

    public static string Post(
        IHeaderDictionary headers,
        Stream body,
        DbDataSource dataSource,
        IDateTimeProvider dateTimeProvider,
        RemoteManager remoteManager
    ) {
        User user = Authorization.AllowByRole(headers, remoteManager, dateTimeProvider, new List<User.Role> { User.Role.WA });

        using DbConnection connection = dataSource.OpenConnection();

        using DbCommand commandPostWaterBody = dataSource.CreateCommand();

        commandPostWaterBody.CommandText = $@"
            select min(on_date), max(on_date), ((consumed*(consumption_sign))+available) as limit, vat_number
            from daily_water_limit
            group by vat_number, ((consumed*(consumption_sign))+available)
        ";

        using DbDataReader reader = commandPostWaterBody.ExecuteReader();
        if (!reader.HasRows)
        {
            return "[]";
        }
        List<PostData> data = new List<PostData>();
        while (reader.Read())
        {
            data.Add(new PostData {
                vat_number = reader.GetString(3),
                limit = reader.GetFloat(2),
                start_date = reader.GetDateTime(0).ToString("yyyy-MM-dd"),
                end_date = reader.GetDateTime(1).ToString("yyyy-MM-dd")
            });
        }
        return JsonSerializer.Serialize(data);
    }

    public static ValueTask<string> Get(
        IHeaderDictionary headers,
        DbDataSource dataSource,
        IDateTimeProvider dateTimeProvider,
        RemoteManager remoteManager
    ) {
        User user = Authorization.AllowByRole(headers, remoteManager, dateTimeProvider, new List<User.Role> { User.Role.WA });

        using DbConnection connection = dataSource.OpenConnection();

        using DbCommand commandGetWaterLimit = dataSource.CreateCommand();

        commandGetWaterLimit.CommandText = $@"
            select available
            from daily_water_limit
            where vat_number = $1 and on_date = $2
            union
            select sum(available_liters + coalesce(qty, 0))
            from offer inner join buy_order
            on offer.id = buy_order.offer_id
            inner join farm_field
            on buy_order.farm_field_id = farm_field.id
            where farm_field.vat_number = $1 and publish_date = $2
        ";
        commandGetWaterLimit.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, user.company_vat_number));
        commandGetWaterLimit.Parameters.Add(DbUtility.CreateParameter(connection, DbType.DateTime, dateTimeProvider.Now.Date));

        using DbDataReader reader = commandGetWaterLimit.ExecuteReader();
        if (!reader.HasRows)
        {
            return new ValueTask<string>("0");
        }
        List<float> data = new List<float>();
        while (reader.Read())
        {
            data.Add(reader.GetFloat(0));
        }
        if(data.Count == 1)
        {
            return new ValueTask<string>(data[0].ToString());
        }
        if(data[0]==0)
        {
            return new ValueTask<string>(data[1].ToString());
        }
        return new ValueTask<string>(data[0].ToString());
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