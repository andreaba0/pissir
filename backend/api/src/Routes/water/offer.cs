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

    public static ValueTask<string> Get(
        IHeaderDictionary headers,
        DbDataSource dataSource,
        IDateTimeProvider dateTimeProvider,
        RemoteManager remoteManager
    )
    {
        User user = Authorization.AllowByRole(headers, remoteManager, dateTimeProvider, new List<User.Role> { User.Role.FA, User.Role.WA });

        using DbConnection connection = dataSource.OpenConnection();

        using DbCommand commandGetWaterOffer = dataSource.CreateCommand();

        if (User.GetRole(user) == User.Role.WA)
        {
            commandGetWaterOffer.CommandText = $@"
                select id, price_liter, available_liters, publish_date
                from offer
                where vat_number = $1 and publish_date >= date_trunc('day', $2) + INTERVAL '1 day'
            ";
            commandGetWaterOffer.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, user.company_vat_number));
            commandGetWaterOffer.Parameters.Add(DbUtility.CreateParameter(connection, DbType.DateTime, dateTimeProvider.UtcNow));
        }
        else
        {
            commandGetWaterOffer.CommandText = $@"
                select id, price_liter, available_liters, publish_date
                from offer
                where publish_date >= date_trunc('day', $1) + INTERVAL '1 day'
            ";
            commandGetWaterOffer.Parameters.Add(DbUtility.CreateParameter(connection, DbType.DateTime, dateTimeProvider.UtcNow));
        }
        using DbDataReader reader = commandGetWaterOffer.ExecuteReader();
        if (!reader.HasRows)
        {
            return new ValueTask<string>("[]");
        }
        List<GetData> data = new List<GetData>();
        while (reader.Read())
        {
            data.Add(new GetData
            {
                id = reader.GetString(0),
                amount = reader.GetFloat(2),
                price = reader.GetFloat(1),
                date = reader.GetDateTime(3)
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

    public struct PostData
    {
        public float amount;
        public float price;
        public string date;
    }

    public static Task PostTransactionWaterOffer(
        DbConnection connection,
        PostData data,
        User user,
        IDateTimeProvider dateTimeProvider,
        DbDataSource dataSource
    )
    {
        using DbCommand commandPostWaterOffer = dataSource.CreateCommand();

        string offerId = Ulid.NewUlid().ToString();

        commandPostWaterOffer.CommandText = $@"
            insert into offer (id, vat_number, price_liter, available_liters, purchased_liters, publish_date)
            values ($1, $2, $3, $4, 0, $5)
            on conflict (vat_number, publish_date, price_liter) do update
            set available_liters = offer.available_liters + $4
        ";
        DateTime date = DateTimeProvider.parseDateFromFrontend(data.date);
        DateTimeOffset d = new DateTimeOffset(date, TimeZoneInfo.Local.GetUtcOffset(date));
        DateTime utcNow = d.UtcDateTime;
        commandPostWaterOffer.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, offerId));
        commandPostWaterOffer.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, user.company_vat_number));
        commandPostWaterOffer.Parameters.Add(DbUtility.CreateParameter(connection, DbType.Single, data.price));
        commandPostWaterOffer.Parameters.Add(DbUtility.CreateParameter(connection, DbType.Single, data.amount));
        commandPostWaterOffer.Parameters.Add(DbUtility.CreateParameter(connection, DbType.DateTime, utcNow));

        commandPostWaterOffer.ExecuteNonQuery();

        return Task.CompletedTask;
    }

    public static Task PostTransaction(
        DbConnection connection,
        PostData data,
        User user,
        IDateTimeProvider dateTimeProvider,
        DbDataSource dataSource
    )
    {
        try
        {
            PostTransactionWaterOffer(connection, data, user, dateTimeProvider, dataSource);
            return Task.CompletedTask;
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
            PostTransactionWaterOffer(connection, data, user, dateTimeProvider, dataSource);
            return Task.CompletedTask;
        }
        catch (DbException ex)
        {
            throw;
        }
    }

    public static ValueTask<string> Post(
        IHeaderDictionary headers,
        Stream body,
        DbDataSource dataSource,
        IDateTimeProvider dateTimeProvider,
        RemoteManager remoteManager
    )
    {
        User user = Authorization.AllowByRole(headers, remoteManager, dateTimeProvider, new List<User.Role> { User.Role.WA });

        PostData data = JsonSerializer.DeserializeAsync<PostData>(body, new JsonSerializerOptions
        {
            IncludeFields = true
        }).Result;

        using DbConnection connection = dataSource.OpenConnection();

        PostTransaction(dataSource.OpenConnection(), data, user, dateTimeProvider, dataSource);

        connection.Close();
        return new ValueTask<string>("Created");
    }
}

public class WaterOfferException : Exception
{
    public enum ErrorCode
    {
        INVALID_BODY,
        OFFER_NOT_CREATED,
        INVALID_DATE
    }

    public ErrorCode Code { get; set; }

    public WaterOfferException(ErrorCode code, string message) : base(message)
    {
        Code = code;
    }
}