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

public class Field
{

    public struct GetData
    {
        public string id;
        public float square_meters;
        public string crop_type;
        public string irrigation_type;
    }

    public static List<GetData> GetField(
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

        using DbCommand commandGetFields = dataSource.CreateCommand();

        commandGetFields.CommandText = $@"
            select id, square_meters, crop_type, irrigation_type
            from farm_field
            where vat_number = $1 and id = $2
        ";

        commandGetFields.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, user.company_vat_number));
        commandGetFields.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, field_id));
        using DbDataReader reader = commandGetFields.ExecuteReader();
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
                square_meters = reader.GetFloat(1),
                crop_type = reader.GetString(2),
                irrigation_type = reader.GetString(3)
            });
        }
        reader.Close();
        connection.Close();
        return data;
    }

    public struct PatchData
    {
        public float square_meters;
        public string crop_type;
        public string irrigation_type;
    }

    public static ValueTask<string> Patch(
        IHeaderDictionary headers,
        RouteValueDictionary routeValues,
        Stream body,
        DbDataSource dataSource,
        IDateTimeProvider dateTimeProvider,
        RemoteManager remoteManager
    )
    {
        User user = Authorization.AllowByRole(headers, remoteManager, dateTimeProvider, new List<User.Role> { User.Role.FA });

        string field_id = routeValues["field_id"].ToString();
        if (field_id == string.Empty)
        {
            throw new FieldException(FieldException.ErrorCode.FIELD_ID_REQUIRED, "Field id required");
        }

        using DbConnection connection = dataSource.OpenConnection();

        PatchData patchData = JsonSerializer.DeserializeAsync<PatchData>(
            body, new JsonSerializerOptions { 
                PropertyNameCaseInsensitive = true,
                IncludeFields = true
        }).Result;

        using DbCommand commandPatchField = dataSource.CreateCommand();
        commandPatchField.CommandText = $@"
                insert into farm_field_versioning (field_id, vat_number, square_meters, crop_type, irrigation_type, created_at)
                values ($1, $2, $3, $4, $5, $6)
            ";
        commandPatchField.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, field_id));
        commandPatchField.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, user.company_vat_number));
        commandPatchField.Parameters.Add(DbUtility.CreateParameter(connection, DbType.Single, patchData.square_meters));
        commandPatchField.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, patchData.crop_type));
        commandPatchField.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, patchData.irrigation_type));
        commandPatchField.Parameters.Add(DbUtility.CreateParameter(connection, DbType.DateTime, dateTimeProvider.UtcNow));
        commandPatchField.ExecuteNonQuery();

        connection.Close();
        return new ValueTask<string>("Field updated");
    }
}