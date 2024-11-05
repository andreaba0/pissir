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
        public DateTime date;
    }

    private static Task UploadTransaction(
        DbConnection connection,
        PostData data
    ) {
        DbCommand transactionCommand = connection.CreateCommand();
        try {
            transactionCommand.CommandText = $@"BEGIN TRANSACTION ISOLATION LEVEL SERIALIZABLE";
            transactionCommand.ExecuteNonQuery();

            transactionCommand.CommandText = $@"
                update offer
                set available_liters = available_liters - $1, purchased_liters = purchased_liters + $1
                where id = $2 and date > CURRENT_DATE and date = $3
            ";
            transactionCommand.Parameters.Add(DbUtility.CreateParameter(connection, DbType.Single, data.amount));
            transactionCommand.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, data.offer_id));
            transactionCommand.Parameters.Add(DbUtility.CreateParameter(connection, DbType.Date, data.date));
            transactionCommand.ExecuteNonQuery();
            transactionCommand.Parameters.Clear();

            transactionCommand.CommandText = $@"
                insert into buy_order (offer_id, farm_field_id, qty)
                values ($1, $2, $3)
                on conflict (offer_id, farm_field_id) do update set qty = qty + $3
            ";
            transactionCommand.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, data.offer_id));
            transactionCommand.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, data.field_id));
            transactionCommand.Parameters.Add(DbUtility.CreateParameter(connection, DbType.Single, data.amount));
            transactionCommand.ExecuteNonQuery();
            transactionCommand.Parameters.Clear();

            transactionCommand.CommandText = $@"
                insert into daily_water_limit (vat_number, consumption_sign, available, consumed, on_date) values
                ($1, -1, 0, $2, $3)
                on conflict (vat_number, on_date) do update set consumed = daily_water_limit.consumed + $2
            ";
            transactionCommand.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, data.field_id));
            transactionCommand.Parameters.Add(DbUtility.CreateParameter(connection, DbType.Single, data.amount));
            transactionCommand.Parameters.Add(DbUtility.CreateParameter(connection, DbType.Date, data.date));
            transactionCommand.ExecuteNonQuery();
            transactionCommand.Parameters.Clear();

            transactionCommand.CommandText = $@"COMMIT";
            transactionCommand.ExecuteNonQuery();

        } catch (DbException e) {
            transactionCommand.CommandText = $@"ROLLBACK";
            transactionCommand.ExecuteNonQuery();
            throw e;
        }

        return Task.CompletedTask;
    }

    public static Task Post(
        IHeaderDictionary headers,
        DbDataSource dataSource,
        IDateTimeProvider dateTimeProvider,
        RemoteManager remoteManager
    )
    {
        User user = Authorization.AllowByRole(headers, remoteManager, dateTimeProvider, new List<User.Role> { User.Role.FA });

        //field_id is not passed in the headers, so we need to get it from the URL
        string field_id = headers["field_id"].Count > 0 ? headers["field_id"].ToString() : string.Empty;
        string offer_id = headers["offer_id"].Count > 0 ? headers["offer_id"].ToString() : string.Empty;
        float amount = headers["amount"].Count > 0 ? float.Parse(headers["amount"].ToString()) : 0;
        DateTime date = headers["date"].Count > 0 ? DateTime.Parse(headers["date"].ToString()) : throw new WaterBuyException(WaterBuyException.ErrorCode.DATE_REQUIRED, "Date required");
        if (field_id == string.Empty) throw new WaterBuyException(WaterBuyException.ErrorCode.FIELD_ID_REQUIRED, "Field id required");
        if (offer_id == string.Empty) throw new WaterBuyException(WaterBuyException.ErrorCode.OFFER_ID_REQUIRED, "Offer id required");
        if (amount == 0) throw new WaterBuyException(WaterBuyException.ErrorCode.AMOUNT_REQUIRED, "Amount required");

        
        using DbConnection connection = dataSource.OpenConnection();

        //call the UploadTransaction method to upload the transaction
        UploadTransaction(connection, new PostData {
            field_id = field_id,
            amount = amount,
            offer_id = offer_id,
            date = date
        });

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
        DATE_REQUIRED
    }

    public ErrorCode Code { get; private set; }

    public WaterBuyException(ErrorCode code, string message) : base(message)
    {
        Code = code;
    }
}