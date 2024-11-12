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

public class WaterOrder
{
    public struct GetData
    {
        public string offer_id;
        public string company_vat_number;
        public string field_id;
        public float quantity;
    }

    public static ValueTask<string> Get(
        IHeaderDictionary headers,
        DbDataSource dataSource,
        IDateTimeProvider dateTimeProvider,
        RemoteManager remoteManager
    )
    {
        User user = Authorization.AllowByRole(headers, remoteManager, dateTimeProvider, new List<User.Role> { User.Role.FA, User.Role.WA });

        using DbConnection connection = dataSource.OpenConnection();

        using DbCommand commandGetSensorData = dataSource.CreateCommand();


        if (User.GetRole(user) == User.Role.FA)
        {
            /*Get a list of all buy orders for each field of the farm belonging to the user*/
            commandGetSensorData.CommandText = $@"
                select offer_id, offer.vat_number, farm_field.id, qty
                from buy_order inner join offer on buy_order.offer_id = offer.id
                inner join farm_field on buy_order.farm_field_id = farm_field.id
                where farm_field.vat_number = $1
            ";
        } else {
            /*Get a list of all the buy orders made by each farm company that belong to the company user works for*/
            commandGetSensorData.CommandText = $@"
                select offer_id, farm_field.vat_number, farm_field.id, qty
                from buy_order inner join offer on buy_order.offer_id = offer.id
                inner join farm_field on buy_order.farm_field_id = farm_field.id
                where offer.vat_number = $1
            ";
        }
        commandGetSensorData.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, user.company_vat_number));

        using DbDataReader reader = commandGetSensorData.ExecuteReader();

        if (!reader.HasRows)
        {
            return new ValueTask<string>("[]");
        }

        List<GetData> data = new List<GetData>();

        while (reader.Read())
        {
            data.Add(new GetData
            {
                offer_id = reader.GetString(0),
                company_vat_number = reader.GetString(1),
                field_id = reader.GetString(2),
                quantity = reader.GetFloat(3)
            });
        }
        reader.Close();
        connection.Close();
        string json = JsonSerializer.Serialize(data, new JsonSerializerOptions
        {
            IncludeFields = true
        });
        return new ValueTask<string>(json);
    }
}