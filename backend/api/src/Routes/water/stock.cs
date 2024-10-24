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

public class WaterStock {

    public struct GetData {
        public string field_id;
        public float limit;
    }
    public static ValueTask<string> Get(
        IHeaderDictionary headers,
        DbDataSource dbDataSource,
        IDateTimeProvider dateTimeProvider,
        RemoteManager remoteManager
    ) {
        User user = Authorization.AllowByRole(headers, remoteManager, dateTimeProvider, new List<User.Role> { User.Role.WA });

        using DbConnection connection = dbDataSource.OpenConnection();
        DbCommand command = dbDataSource.CreateCommand();
        command.CommandText = $@"
            select farm_field_id, qty
            from buy_order inner join offer
            on buy_order.offer_id = offer.id
            where vat_number = $1 and publish_date = $2
        ";
        command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, user.company_vat_number));
        command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.DateTime, dateTimeProvider.Now.Date));
        using DbDataReader reader = command.ExecuteReader();
        if (!reader.HasRows)
        {
            return new ValueTask<string>("[]");
        }
        List<GetData> data = new List<GetData>();
        while (reader.Read())
        {
            GetData item = new GetData() {
                field_id = reader.GetString(0),
                limit = reader.GetFloat(1)
            };
        }
        return new ValueTask<string>(JsonSerializer.Serialize(data, new JsonSerializerOptions {
            IncludeFields = true
        }));
    }
}