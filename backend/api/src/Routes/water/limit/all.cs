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

        if(User.GetRole(user) == User.Role.FA)
        {
            commandGetWaterLimitAll.CommandText = $@"
                select on_date, consumed+available as limit, vat_number
                from daily_water_limit
                where consumption_sign=1 and vat_number=$1
                order by on_date asc
            ";
            commandGetWaterLimitAll.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, user.company_vat_number));
        }
        else
        {
            commandGetWaterLimitAll.CommandText = $@"
                select on_date, consumed+available as limit, vat_number
                from daily_water_limit
                where consumption_sign=1
                order by on_date asc
            ";
        }

        using DbDataReader reader = commandGetWaterLimitAll.ExecuteReader();
        if (!reader.HasRows)
        {
            return "[]";
        }
        List<WaterLimitGroup.Record> data = new List<WaterLimitGroup.Record>();
        while (reader.Read())
        {
            data.Add(new WaterLimitGroup.Record(
                reader.GetString(2),
                reader.GetFloat(1),
                reader.GetDateTime(0)
            ));
        }
        reader.Close();
        connection.Close();

        List<WaterLimitGroup.GroupedData> groupedData = WaterLimitGroup.GroupData(data);
        
        string json = JsonSerializer.Serialize(groupedData, new JsonSerializerOptions { 
            IncludeFields = true
        });
        return json;
    }
}