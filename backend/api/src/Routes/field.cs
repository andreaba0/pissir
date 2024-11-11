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

public class Fields
{

    public struct GetData
    {
        public string id;
        public float square_meters;
        public string crop_type;
        public string irrigation_type;
    }

    public struct PostData
    {
        public float square_meters;
        public string crop_type;
        public string irrigation_type;
    }

    internal static Task PostTransactionField(
        DbConnection connection,
        PostData data,
        IDateTimeProvider dateTimeProvider,
        DbDataSource dataSource,
        User user
    )
    {
        using DbCommand commandPostField = dataSource.CreateCommand();

        commandPostField.CommandText = "BEGIN TRANSACTION";
        commandPostField.ExecuteNonQuery();

        string fieldId = Ulid.NewUlid().ToString();

        commandPostField.CommandText = $@"
            insert into farm_field (id, vat_number) values
            ($1, $2)
        ";
        commandPostField.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, fieldId));
        commandPostField.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, user.company_vat_number));
        commandPostField.ExecuteNonQuery();
        commandPostField.Parameters.Clear();

        commandPostField.CommandText = $@"
            insert into farm_field_versioning (field_id, vat_number, square_meters, crop_type, irrigation_type, created_at)
            values ($1, $2, $3, $4, $5, $6)
        ";
        commandPostField.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, fieldId));
        commandPostField.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, user.company_vat_number));
        commandPostField.Parameters.Add(DbUtility.CreateParameter(connection, DbType.Single, data.square_meters));
        commandPostField.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, data.crop_type));
        commandPostField.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, data.irrigation_type));
        commandPostField.Parameters.Add(DbUtility.CreateParameter(connection, DbType.DateTime, dateTimeProvider.UtcNow));
        commandPostField.ExecuteNonQuery();

        commandPostField.CommandText = "COMMIT TRANSACTION";
        commandPostField.ExecuteNonQuery();

        return Task.CompletedTask;
    }

    internal static Task PostTransaction(
        DbConnection connection,
        PostData data,
        IDateTimeProvider dateTimeProvider,
        DbDataSource dataSource,
        User user
    )
    {
        Exception exception;
        try
        {
            PostTransactionField(connection, data, dateTimeProvider, dataSource, user);
            return Task.CompletedTask;
        }
        catch (DbException ex)
        {
            DbCommand commandPostField = dataSource.CreateCommand();
            commandPostField.CommandText = "ROLLBACK TRANSACTION";
            commandPostField.ExecuteNonQuery();
            if (!AuthenticatedPostTransaction.ExceptionMatchCompanyNotFound(ex))
            {
                throw;
            }
        }
        catch (Exception ex)
        {
            throw;
        }
        try
        {
            AuthenticatedPostTransaction.CreateUserInDatabase(dataSource, user);
            PostTransactionField(connection, data, dateTimeProvider, dataSource, user);
            return Task.CompletedTask;
        }
        catch (DbException ex)
        {
            DbCommand commandPostField = dataSource.CreateCommand();
            commandPostField.CommandText = "ROLLBACK TRANSACTION";
            commandPostField.ExecuteNonQuery();
            throw;
        }
    }

    public static ValueTask<string> PostField(
        IHeaderDictionary headers,
        Stream body,
        DbDataSource dataSource,
        IDateTimeProvider dateTimeProvider,
        RemoteManager remoteManager
    )
    {
        User user = Authorization.AllowByRole(headers, remoteManager, dateTimeProvider, new List<User.Role> { User.Role.FA });

        PostData data = JsonSerializer.DeserializeAsync<PostData>(body, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            IncludeFields = true
        }).Result;

        using DbConnection connection = dataSource.OpenConnection();
        PostTransaction(connection, data, dateTimeProvider, dataSource, user);
        connection.Close();


        return new ValueTask<string>("Created");
    }

    public static string GetFields(
        IHeaderDictionary headers,
        DbDataSource dataSource,
        IDateTimeProvider dateTimeProvider,
        RemoteManager remoteManager
    )
    {
        User user = Authorization.AllowByRole(headers, remoteManager, dateTimeProvider, new List<User.Role> { User.Role.FA });

        using DbConnection connection = dataSource.OpenConnection();

        using DbCommand commandGetFields = dataSource.CreateCommand();

        commandGetFields.CommandText = $@"
            select distinct on (farm_field.id) farm_field.id, square_meters, crop_type, irrigation_type
            from farm_field inner join farm_field_versioning
            on farm_field.id = farm_field_versioning.field_id
            where farm_field.vat_number = $1
            order by farm_field.id, farm_field_versioning.created_at desc
        ";

        commandGetFields.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, user.company_vat_number));
        using DbDataReader reader = commandGetFields.ExecuteReader();
        if (!reader.HasRows)
        {
            return "[]";
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

        string json = JsonSerializer.Serialize(data, new JsonSerializerOptions
        {
            IncludeFields = true
        });
        return json;
    }
}

public class FieldException : Exception
{
    public enum ErrorCode
    {
        GENERIC_ERROR = 0,
        MISSING_FIELD = 1,
        UNKNOW_FIELD_TYPE = 2,
        BODY_PARSE_ERROR = 3,
        INVALID_FIELD_ID = 4,
        FIELD_ID_REQUIRED = 5
    }
    public ErrorCode Code { get; } = default(ErrorCode);
    public FieldException(ErrorCode errorCode, string message) : base(message)
    {
        Code = errorCode;
    }
}