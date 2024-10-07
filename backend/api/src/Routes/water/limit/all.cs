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

public class WaterLimitAll
{
    public struct GetData {
        public string vat_number;
        public float limit;
        public DateTime start_date;
        public DateTime end_date;
    }

    public static string GetWaterLimitAll(
        IHeaderDictionary headers,
        DbDataSource dataSource,
        IDateTimeProvider dateTimeProvider,
        RemoteManager remoteManager
    )
    {
        User user = Authorization.AllowByRole(headers, remoteManager, dateTimeProvider, new List<User.Role> { User.Role.FA, User.Role.WA });

        using DbConnection connection = dataSource.OpenConnection();

        using DbCommand commandGetWaterLimitAll = dataSource.CreateCommand();

        commandGetWaterLimitAll.CommandText = $@"
            select min(on_date), max(on_date), ((consumed*(consumption_sign))+available) as limit, vat_number
            from daily_water_limit
            group by vat_number, ((consumed*(consumption_sign))+available)
        ";

        using DbDataReader reader = commandGetWaterLimitAll.ExecuteReader();
        if (!reader.HasRows)
        {
            return "[]";
        }
        List<GetData> data = new List<GetData>();
        while (reader.Read())
        {
            data.Add(new GetData {
                vat_number = reader.GetString(3),
                limit = reader.GetFloat(2),
                start_date = reader.GetDateTime(0),
                end_date = reader.GetDateTime(1)
            });
        }
        reader.Close();
        connection.Close();
        
        string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { 
            IncludeFields = true
        });
        return json;
    }
}