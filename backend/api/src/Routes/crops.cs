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

public class Crops
{

    public static string GetCrops(
        IHeaderDictionary headers,
        DbDataSource dataSource,
        IDateTimeProvider dateTimeProvider,
        RemoteManager remoteManager
    )
    {
        User user = Authorization.AllowByRole(headers, remoteManager, dateTimeProvider, new List<User.Role> { User.Role.FA, User.Role.WA });

        using DbConnection connection = dataSource.OpenConnection();

        using DbCommand commandGetCrops = dataSource.CreateCommand();

        commandGetCrops.CommandText = $@"
            select crop
            from consumption_fact
            order by crop asc
        ";

        using DbDataReader reader = commandGetCrops.ExecuteReader();
        if (!reader.HasRows)
        {
            return "[]";
        }
        List<string> data = new List<string>();
        while (reader.Read())
        {
            data.Add(reader.GetString(0));
        }
        reader.Close();
        connection.Close();
        
        string json = JsonSerializer.Serialize(data);
        return json;
    }
}