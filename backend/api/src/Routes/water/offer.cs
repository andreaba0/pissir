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

    public static ValueTask<string> Post(
        IHeaderDictionary headers,
        Stream body,
        DbDataSource dataSource,
        IDateTimeProvider dateTimeProvider,
        RemoteManager remoteManager
    )
    {
        User user = Authorization.AllowByRole(headers, remoteManager, dateTimeProvider, new List<User.Role> { User.Role.WA });

        PostData data = JsonSerializer.Deserialize<PostData>(body, new JsonSerializerOptions
        {
            IncludeFields = true
        });

        DateTime date = DateTimeProvider.parseDateFromFrontend(data.date);

        if (date < dateTimeProvider.UtcNow)
        {
            throw new WaterOfferException(WaterOfferException.ErrorCode.INVALID_DATE, "Date must be greater than current date");
        }

        using DbConnection connection = dataSource.OpenConnection();

        using DbCommand commandPostWaterOffer = dataSource.CreateCommand();

        commandPostWaterOffer.CommandText = $@"
            insert into offer (id, vat_number, price_liter, available_liters, purchased_liters publish_date)
            values ($1, $2, $3, $4, 0, $5)
            returning id
        ";
        commandPostWaterOffer.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, Guid.NewGuid().ToString()));
        commandPostWaterOffer.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, user.company_vat_number));
        commandPostWaterOffer.Parameters.Add(DbUtility.CreateParameter(connection, DbType.Single, data.price));
        commandPostWaterOffer.Parameters.Add(DbUtility.CreateParameter(connection, DbType.Single, data.amount));
        commandPostWaterOffer.Parameters.Add(DbUtility.CreateParameter(connection, DbType.DateTime, data.date));

        using DbDataReader reader = commandPostWaterOffer.ExecuteReader();
        if (!reader.HasRows)
        {
            throw new WaterOfferException(WaterOfferException.ErrorCode.OFFER_NOT_CREATED, "Offer not created");
        }
        reader.Read();
        string offerId = reader.GetString(0);
        reader.Close();
        connection.Close();
        return new ValueTask<string>(offerId);
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