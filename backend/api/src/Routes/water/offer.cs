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

public class WaterOffer
{

    public struct GetData
    {
        public string id;
        public float amount;
        public float price;
        public DateTime date;
    }

    public static List<GetData> GetWaterOffer(
        IHeaderDictionary headers,
        DbDataSource dataSource,
        IDateTimeProvider dateTimeProvider,
        RemoteManager remoteManager
    )
    {
        User user = Authorization.AllowByRole(headers, remoteManager, dateTimeProvider, new List<User.Role> { User.Role.FA });

        //field_id is not passed in the headers, so we need to get it from the URL
        string field_id = headers["field_id"].Count > 0 ? headers["field_id"].ToString() : string.Empty;
        if (field_id == string.Empty)
        {
            throw new FieldException(FieldException.ErrorCode.FIELD_ID_REQUIRED, "Field id required");
        }

        using DbConnection connection = dataSource.OpenConnection();

        using DbCommand commandGetWaterOffer = dataSource.CreateCommand();

        commandGetWaterOffer.CommandText = $@"
            select id, price_liter, available_liters, publish_date
            from offer
            where publish_date >= CURRENT_DATE + INTERVAL '1 day'
        ";
        using DbDataReader reader = commandGetWaterOffer.ExecuteReader();
        if (!reader.HasRows)
        {
            return new List<GetData>();
        }
        List<GetData> data = new List<GetData>();
        while (reader.Read())
        {
            data.Add(new GetData
            {
                id = reader.GetString(0),
                amount = reader.GetFloat(1),
                price = reader.GetFloat(2),
                date = reader.GetDateTime(3)
            });
        }
        reader.Close();
        connection.Close();
        return data;
    }
}