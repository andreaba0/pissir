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

public class ResourceManagerWaterStock {
    public struct GetData {
        public string field_id;
        public float limit;
    }

    public static ValueTask<string> Get(
        IHeaderDictionary headers,
        RouteValueDictionary routeValues,
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
        string field_id = routeValues["field_id"].ToString();

        using DbConnection connection = dataSource.OpenConnection();
        using DbCommand commandGetFields = dataSource.CreateCommand();

        commandGetFields.CommandText = $@"
            select sum(qty)
            from buy_order inner join farm_field
            on buy_order.farm_field_id = farm_field.id
            inner join offer
            on offer.id = buy_order.offer_id
            where farm_field.vat_number = $1 and farm_field.id = $2 and date_trunc('day', offer.publish_date) = date_trunc('day', $3)
        ";
        commandGetFields.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, vat_number));
        commandGetFields.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, field_id));
        commandGetFields.Parameters.Add(DbUtility.CreateParameter(connection, DbType.DateTime, dateTimeProvider.UtcNow));
        using DbDataReader reader = commandGetFields.ExecuteReader();
        if (!reader.HasRows) {
            reader.Close();
            connection.Close();
            return new ValueTask<string>("[]");
        }
        reader.Read();
        GetData res = new GetData {
            field_id = field_id,
            limit = reader.GetFloat(0)
        };
        reader.Close();
        connection.Close();

        return new ValueTask<string>(JsonSerializer.Serialize(res, new JsonSerializerOptions {
            IncludeFields = true
        }));
    }
}