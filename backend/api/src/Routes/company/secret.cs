using System;
using System.Security.Cryptography;
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

public class CompanySecret
{
    public enum KeyState { CREATED, EXISTING }
    public struct PostResponse {
        public string secret_key;
        public KeyState state;
    }

    public static PostResponse Post(
        IHeaderDictionary headers,
        DbDataSource dataSource,
        IDateTimeProvider dateTimeProvider,
        RemoteManager remoteManager
    )
    {
        User user = Authorization.AllowByRole(headers, remoteManager, dateTimeProvider, new List<User.Role> { User.Role.FA });

        // always generate a new secret_key to insert into the database if it does not exist. If it exists, old one is returned. 
        string key = Convert.ToBase64String(new HMACSHA256().Key);

        using DbConnection connection = dataSource.OpenConnection();

        using DbCommand commandGetSecretKey = dataSource.CreateCommand();

        DateTime now = dateTimeProvider.Now;

        // <insert ... on conflict(...) do update ... returning ...> is used to always return the secret_key
        // Without this command, a transaction would be needed:
        // 1> select ... for update; 
        // 2> insert ...;
        commandGetSecretKey.CommandText = $@"
            select coalesce(
                (insert into secret_key (
                    company_vat_number,
                    secret_key,
                    created_at
                ) values (
                    $1,
                    $2,
                    $3
                ) on conflict (company_vat_number) do nothing returning secret_key, created_at),
                (select secret_key, created_at from secret_key where company_vat_number = $1)
            ) as secret_key
        ";

        commandGetSecretKey.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, user.company_vat_number));
        commandGetSecretKey.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, key));
        commandGetSecretKey.Parameters.Add(DbUtility.CreateParameter(connection, DbType.DateTime, now));

        using DbDataReader reader = commandGetSecretKey.ExecuteReader();
        if (!reader.HasRows)
        {
            throw new Exception("Server side error");
        }
        string secret_key = "";
        reader.Read();
        string secret = reader.GetString(0);
        DateTime created_at = reader.GetDateTime(1);
        
        reader.Close();
        connection.Close();
        
        if(created_at == now) return new PostResponse() {
            secret_key= secret,
            state= KeyState.EXISTING
        };

        return new PostResponse() {
            secret_key = secret,
            state=KeyState.CREATED
        };
    }
}