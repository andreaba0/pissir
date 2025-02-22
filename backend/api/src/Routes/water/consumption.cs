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
using Npgsql;

namespace Routes;

public class WaterConsumption
{
    public struct GetData
    {
        public string field_id;
        public DateTime data;
        public float amount_used;
        public float amount_ordered;
    }
    public static ValueTask<string> Get(
        IHeaderDictionary headers,
        DbDataSource dbDataSource,
        IDateTimeProvider dateTimeProvider,
        RemoteManager remoteManager
    )
    {
        User user = Authorization.AllowByRole(headers, remoteManager, dateTimeProvider, new List<User.Role> { User.Role.WA, User.Role.FA });

        using DbConnection connection = dbDataSource.OpenConnection();
        DbCommand command = dbDataSource.CreateCommand();
        if (User.GetRole(user) == User.Role.FA)
        {
            command.CommandText = $@"
                select farm_field.id, coalesce(sum(buy_order.qty), 0) as amount_ordered, offer.publish_date as publish_date, coalesce((
                    select coalesce(sum(water_used), 0) as amount_used
                    from actuator_log as al inner join object_logger as ol on al.object_id=ol.id
                    where ol.farm_field_id = farm_field.id and date_trunc('day', al.log_time) = date_trunc('day', offer.publish_date)
                ), 0) as amount_used
                from buy_order inner join offer
                on buy_order.offer_id = offer.id
                inner join farm_field on buy_order.farm_field_id = farm_field.id
                where farm_field.vat_number = $1
                group by farm_field.id, offer.publish_date
            ";
            command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, user.company_vat_number));
        }
        else
        {
            command.CommandText = $@"
                select farm_field.id, coalesce(sum(buy_order.qty), 0) as amount_ordered, offer.publish_date as publish_date, coalesce((
                    select coalesce(sum(water_used), 0) as amount_used
                    from actuator_log as al inner join object_logger as ol on al.object_id=ol.id
                    where ol.farm_field_id = farm_field.id and date_trunc('day', al.log_time) = date_trunc('day', offer.publish_date)
                ), 0) as amount_used
                from buy_order inner join offer
                on buy_order.offer_id = offer.id
                inner join farm_field on buy_order.farm_field_id = farm_field.id
                where offer.vat_number = $1
                group by farm_field.id, offer.publish_date
            ";
            command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, user.company_vat_number));
        }
        using DbDataReader reader = command.ExecuteReader();
        if (!reader.HasRows)
        {
            return new ValueTask<string>("[]");
        }
        List<GetData> data = new List<GetData>();
        while (reader.Read())
        {
            GetData item = new GetData()
            {
                field_id = reader.GetString(0),
                data = reader.GetDateTime(2),
                amount_ordered = reader.GetFloat(1),
                amount_used = reader.GetFloat(3)
            };
            data.Add(item);
        }
        string json = JsonSerializer.Serialize(data, new JsonSerializerOptions
        {
            IncludeFields = true
        });
        return new ValueTask<string>(json);
    }
}