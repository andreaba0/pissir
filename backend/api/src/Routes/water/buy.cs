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

public class WaterBuy
{

    public struct PostData
    {
        public string field_id;
        public float amount;
        public string offer_id;
        public string date;
    }

    private static Task UploadTransaction(
        DbConnection connection,
        PostData data,
        User user,
        IDateTimeProvider dateTimeProvider
    ) {
        DbCommand transactionCommand = connection.CreateCommand();
        try {
            transactionCommand.CommandText = $@"BEGIN TRANSACTION ISOLATION LEVEL SERIALIZABLE";
            transactionCommand.ExecuteNonQuery();

            transactionCommand.CommandText = $@"
                update offer
                set available_liters = available_liters - $1, purchased_liters = purchased_liters + $1
                where id = $2 and date_trunc('day', publish_date) > date_trunc('day', $3)
                returning id, publish_date
            ";
            transactionCommand.Parameters.Add(DbUtility.CreateParameter(connection, DbType.Single, data.amount));
            transactionCommand.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, data.offer_id));
            transactionCommand.Parameters.Add(DbUtility.CreateParameter(connection, DbType.Date, dateTimeProvider.UtcNow.Date));
            using DbDataReader reader1 = transactionCommand.ExecuteReader();
            if (!reader1.HasRows) {
                transactionCommand.CommandText = $@"ROLLBACK";
                transactionCommand.ExecuteNonQuery();
                throw new WaterBuyException(WaterBuyException.ErrorCode.NO_OFFER_AVAILABLE, "No offer available");
            }
            reader1.Read();
            DateTime publishDate = reader1.GetDateTime(1);
            reader1.Close();

            transactionCommand.Parameters.Clear();

            transactionCommand.CommandText = $@"
                insert into buy_order (offer_id, farm_field_id, qty)
                values ($1, $2, $3)
                on conflict (offer_id, farm_field_id) do update set qty = buy_order.qty + $3
                returning offer_id
            ";
            transactionCommand.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, data.offer_id));
            transactionCommand.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, data.field_id));
            transactionCommand.Parameters.Add(DbUtility.CreateParameter(connection, DbType.Single, data.amount));
            using DbDataReader reader2 = transactionCommand.ExecuteReader();
            if (!reader2.HasRows) {
                transactionCommand.CommandText = $@"ROLLBACK";
                transactionCommand.ExecuteNonQuery();
                throw new WaterBuyException(WaterBuyException.ErrorCode.NO_OFFER_FOUND, "No offer found");
            }
            reader2.Close();
            transactionCommand.Parameters.Clear();

            transactionCommand.CommandText = $@"
                insert into daily_water_limit (vat_number, consumption_sign, available, consumed, on_date) values
                ($1, -1, 0, $2, $3)
                on conflict (vat_number, on_date) do update set consumed = daily_water_limit.consumed + $2
                returning vat_number
            ";
            transactionCommand.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, user.company_vat_number));
            transactionCommand.Parameters.Add(DbUtility.CreateParameter(connection, DbType.Single, data.amount));
            transactionCommand.Parameters.Add(DbUtility.CreateParameter(connection, DbType.Date, publishDate));
            using DbDataReader reader3 = transactionCommand.ExecuteReader();
            if (!reader3.HasRows) {
                transactionCommand.CommandText = $@"ROLLBACK";
                transactionCommand.ExecuteNonQuery();
                throw new WaterBuyException(WaterBuyException.ErrorCode.NO_OFFER_FOUND, "No offer found");
            }
            reader3.Close();
            transactionCommand.Parameters.Clear();

            transactionCommand.CommandText = $@"COMMIT";
            transactionCommand.ExecuteNonQuery();

        } catch (DbException e) {
            Console.WriteLine(e);
            transactionCommand.CommandText = $@"ROLLBACK";
            transactionCommand.ExecuteNonQuery();
            throw;
        }

        return Task.CompletedTask;
    }

    public static Task Post(
        IHeaderDictionary headers,
        Stream body,
        DbDataSource dataSource,
        IDateTimeProvider dateTimeProvider,
        RemoteManager remoteManager
    )
    {
        User user = Authorization.AllowByRole(headers, remoteManager, dateTimeProvider, new List<User.Role> { User.Role.FA });

        //field_id is not passed in the headers, so we need to get it from the URL
        /*string field_id = headers["field_id"].Count > 0 ? headers["field_id"].ToString() : string.Empty;
        string offer_id = headers["offer_id"].Count > 0 ? headers["offer_id"].ToString() : string.Empty;
        float amount = headers["amount"].Count > 0 ? float.Parse(headers["amount"].ToString()) : 0;
        DateTime date = headers["date"].Count > 0 ? DateTime.Parse(headers["date"].ToString()) : throw new WaterBuyException(WaterBuyException.ErrorCode.DATE_REQUIRED, "Date required");
        if (field_id == string.Empty) throw new WaterBuyException(WaterBuyException.ErrorCode.FIELD_ID_REQUIRED, "Field id required");
        if (offer_id == string.Empty) throw new WaterBuyException(WaterBuyException.ErrorCode.OFFER_ID_REQUIRED, "Offer id required");
        if (amount == 0) throw new WaterBuyException(WaterBuyException.ErrorCode.AMOUNT_REQUIRED, "Amount required");
*/
        PostData postData = JsonSerializer.DeserializeAsync<PostData>(body, new JsonSerializerOptions{
            PropertyNameCaseInsensitive = true,
            IncludeFields = true
        }).Result;

        string field_id = postData.field_id;
        string offer_id = postData.offer_id;
        float amount = postData.amount;
        DateTime date = DateTimeProvider.parseDateFromFrontend(postData.date);
        
        using DbConnection connection = dataSource.OpenConnection();

        //call the UploadTransaction method to upload the transaction
        UploadTransaction(connection, postData, user, dateTimeProvider);

        connection.Close();

        return Task.CompletedTask;
    }
}

public class WaterBuyException : Exception
{
    public enum ErrorCode
    {
        FIELD_ID_REQUIRED,
        OFFER_ID_REQUIRED,
        AMOUNT_REQUIRED,
        DATE_REQUIRED,
        NO_OFFER_AVAILABLE,
        NO_OFFER_FOUND
    }

    public ErrorCode Code { get; private set; }

    public WaterBuyException(ErrorCode code, string message) : base(message)
    {
        Code = code;
    }
}