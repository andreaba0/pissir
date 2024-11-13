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
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Web;
using System.Text.RegularExpressions;

namespace Routes;

public class CompanySecret
{
    public enum KeyState { CREATED, EXISTING }
    public struct PostResponse
    {
        public string secret_key;
        public KeyState state;
    }

    public static PostResponse PostTransactionSecret(
        DbConnection connection,
        string secret,
        User user,
        IDateTimeProvider dateTimeProvider,
        DbDataSource dataSource
    )
    {
        try
        {
            using DbCommand commandPostSecret = dataSource.CreateCommand();

            commandPostSecret.CommandText = "BEGIN TRANSACTION";
            commandPostSecret.ExecuteNonQuery();

            commandPostSecret.CommandText = $@"select secret_key from secret_key where company_vat_number = $1";
            commandPostSecret.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, user.company_vat_number));

            using DbDataReader reader = commandPostSecret.ExecuteReader();
            if (reader.HasRows)
            {
                reader.Read();
                string secret_key = reader.GetString(0);
                reader.Close();
                commandPostSecret.CommandText = "ROLLBACK";
                commandPostSecret.ExecuteNonQuery();
                return new PostResponse()
                {
                    secret_key = secret_key,
                    state = KeyState.EXISTING
                };
            }
            reader.Close();

            commandPostSecret.Parameters.Clear();
            commandPostSecret.CommandText = $@"
                insert into secret_key (company_vat_number, secret_key, created_at) values
                ($1, $2, $3)
            ";
            commandPostSecret.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, user.company_vat_number));
            commandPostSecret.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, secret));
            commandPostSecret.Parameters.Add(DbUtility.CreateParameter(connection, DbType.DateTime, dateTimeProvider.UtcNow));

            commandPostSecret.ExecuteNonQuery();

            commandPostSecret.CommandText = "COMMIT";
            commandPostSecret.ExecuteNonQuery();

            return new PostResponse()
            {
                secret_key = secret,
                state = KeyState.CREATED
            };
        }
        catch (DbException ex)
        {
            Exception e = ex;
            DbCommand commandRollback = dataSource.CreateCommand();
            commandRollback.CommandText = "ROLLBACK";
            commandRollback.ExecuteNonQuery();
            throw e;
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public static Task<PostResponse> PostTransaction(
        DbConnection connection,
        string secret,
        User user,
        IDateTimeProvider dateTimeProvider,
        DbDataSource dataSource
    )
    {
        try
        {
            PostResponse response = PostTransactionSecret(connection, secret, user, dateTimeProvider, dataSource);
            return Task.FromResult(response);
        }
        catch (DbException ex)
        {
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
            PostResponse response = PostTransactionSecret(connection, secret, user, dateTimeProvider, dataSource);
            return Task.FromResult(response);
        }
        catch (DbException ex)
        {
            throw;
        }
    }

    public static string PasswordGenerator(int length) {
        const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
        StringBuilder res = new StringBuilder();
        using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
        {
            byte[] uintBuffer = new byte[sizeof(uint)];

            while (length-- > 0)
            {
                rng.GetBytes(uintBuffer);
                uint num = BitConverter.ToUInt32(uintBuffer, 0);
                res.Append(valid[(int)(num % (uint)valid.Length)]);
            }
        }
        return res.ToString();
    }

    public static PostResponse Post(
        string cookie,
        DbDataSource dataSource,
        IDateTimeProvider dateTimeProvider,
        RemoteManager remoteManager
    )
    {
        Token token = Authentication.VerifiedPayload(cookie, remoteManager, dateTimeProvider);
        User user = new User(
            global_id: token.sub,
            role: token.role,
            company_vat_number: token.company_vat_number
        );
        if (User.GetRole(user) != User.Role.FA)
        {
            throw new AuthorizationException(AuthorizationException.ErrorCode.UNAUTHORIZED, "Unauthorized");
        }

        // always generate a new secret_key to insert into the database if it does not exist. If it exists, old one is returned. 
        string key = PasswordGenerator(16);

        using DbConnection connection = dataSource.OpenConnection();

        PostResponse response = PostTransaction(connection, key, user, dateTimeProvider, dataSource).Result;

        connection.Close();

        return response;
    }
}