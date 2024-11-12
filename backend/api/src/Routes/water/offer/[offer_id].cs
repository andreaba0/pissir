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

public class WaterOfferOfferId
{
    public enum DeleteResponse
    {
        OfferNotDeleted,
        OfferDeleted,
        OfferNotFound
    }

    public static ValueTask<DeleteResponse> DeleteTransaction(
        DbConnection connection,
        DbDataSource dbDataSource,
        IDateTimeProvider dateTimeProvider,
        string offerId,
        User user
    )
    {
        try
        {
            DbCommand commandInitTransaction = dbDataSource.CreateCommand();
            commandInitTransaction.CommandText = "begin transaction isolation level read committed";
            commandInitTransaction.ExecuteNonQuery();


            DbCommand command = dbDataSource.CreateCommand();
            command.CommandText = $@"
            delete from offer
            where id = $1 and vat_number = $2 and date_trunc('day', publish_date + interval '1 day') >= $3
            returning id
        ";
            command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, offerId));
            command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, user.company_vat_number));
            command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.DateTime, dateTimeProvider.UtcNow));

            using DbDataReader reader = command.ExecuteReader();
            if (reader.HasRows)
            {
                DbCommand commandCommitTransaction = dbDataSource.CreateCommand();
                commandCommitTransaction.CommandText = "commit";
                commandCommitTransaction.ExecuteNonQuery();
                return new ValueTask<DeleteResponse>(DeleteResponse.OfferDeleted);
            }
            
            DbCommand commandGetOffer = dbDataSource.CreateCommand();
            commandGetOffer.CommandText = $@"
                select publish_date
                from offer
                where id = $1
            ";
            commandGetOffer.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, offerId));
            using DbDataReader readerGetOffer = commandGetOffer.ExecuteReader();
            if (!readerGetOffer.HasRows)
            {
                DbCommand commandRollbackTransaction = dbDataSource.CreateCommand();
                commandRollbackTransaction.CommandText = "rollback";
                commandRollbackTransaction.ExecuteNonQuery();
                return new ValueTask<DeleteResponse>(DeleteResponse.OfferNotFound);
            }

            DbCommand commandRollbackTransaction2 = dbDataSource.CreateCommand();
            commandRollbackTransaction2.CommandText = "rollback";
            commandRollbackTransaction2.ExecuteNonQuery();
            return new ValueTask<DeleteResponse>(DeleteResponse.OfferNotDeleted);
        } catch(DbException e) {
            DbCommand commandRollbackTransaction = dbDataSource.CreateCommand();
            commandRollbackTransaction.CommandText = "rollback";
            commandRollbackTransaction.ExecuteNonQuery();
            connection.Close();
            throw;
        }
    }

    public static ValueTask<DeleteResponse> Delete(
        IHeaderDictionary headers,
        RouteValueDictionary routeValues,
        DbDataSource dbDataSource,
        IDateTimeProvider dateTimeProvider,
        RemoteManager remoteManager
    )
    {
        User user = Authorization.AllowByRole(headers, remoteManager, dateTimeProvider, new List<User.Role> { User.Role.WA });

        string offerId = routeValues["offer_id"].ToString();

        DeleteResponse DeleteResponse = DeleteTransaction(dbDataSource.OpenConnection(), dbDataSource, dateTimeProvider, offerId, user).Result;

        return new ValueTask<DeleteResponse>(DeleteResponse);
    }

    public enum PatchResponse {
        OfferNotFound,
        OfferNotPatched,
        OfferPatched
    }

    public struct PatchData {
        public float update_amount_to;
    }

    public static ValueTask<PatchResponse> PatchTransaction(
        DbConnection connection,
        DbDataSource dbDataSource,
        IDateTimeProvider dateTimeProvider,
        string offerId,
        User user,
        PatchData data
    )
    {
        try
        {
            DbCommand commandInitTransaction = dbDataSource.CreateCommand();
            commandInitTransaction.CommandText = "begin transaction isolation level read committed";
            commandInitTransaction.ExecuteNonQuery();


            DbCommand command = dbDataSource.CreateCommand();
            command.CommandText = $@"
            update offer
            set available_liters = $1
            where id = $2 and vat_number = $3 and date_trunc('day', publish_date + interval '1 day') >= $4
            returning id
        ";
            command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.Single, data.update_amount_to));
            command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, offerId));
            command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, user.company_vat_number));
            command.Parameters.Add(DbUtility.CreateParameter(connection, DbType.DateTime, dateTimeProvider.UtcNow));

            using DbDataReader reader = command.ExecuteReader();
            if (reader.HasRows)
            {
                DbCommand commandCommitTransaction = dbDataSource.CreateCommand();
                commandCommitTransaction.CommandText = "commit";
                commandCommitTransaction.ExecuteNonQuery();
                return new ValueTask<PatchResponse>(PatchResponse.OfferPatched);
            }
            
            DbCommand commandGetOffer = dbDataSource.CreateCommand();
            commandGetOffer.CommandText = $@"
                select publish_date
                from offer
                where id = $1
            ";
            commandGetOffer.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, offerId));
            using DbDataReader readerGetOffer = commandGetOffer.ExecuteReader();
            if (!readerGetOffer.HasRows)
            {
                DbCommand commandRollbackTransaction = dbDataSource.CreateCommand();
                commandRollbackTransaction.CommandText = "rollback";
                commandRollbackTransaction.ExecuteNonQuery();
                return new ValueTask<PatchResponse>(PatchResponse.OfferNotFound);
            }

            DbCommand commandRollbackTransaction2 = dbDataSource.CreateCommand();
            commandRollbackTransaction2.CommandText = "rollback";
            commandRollbackTransaction2.ExecuteNonQuery();
            return new ValueTask<PatchResponse>(PatchResponse.OfferNotPatched);
        } catch(DbException e) {
            DbCommand commandRollbackTransaction = dbDataSource.CreateCommand();
            commandRollbackTransaction.CommandText = "rollback";
            commandRollbackTransaction.ExecuteNonQuery();
            connection.Close();
            throw;
        }
    }

    public static ValueTask<PatchResponse> Patch(
        IHeaderDictionary headers,
        Stream body,
        RouteValueDictionary routeValues,
        DbDataSource dbDataSource,
        IDateTimeProvider dateTimeProvider,
        RemoteManager remoteManager
    )
    {
        User user = Authorization.AllowByRole(headers, remoteManager, dateTimeProvider, new List<User.Role> { User.Role.WA });

        string offerId = routeValues["offer_id"].ToString();

        PatchData data = JsonSerializer.DeserializeAsync<PatchData>(body, new JsonSerializerOptions {
            IncludeFields = true
        }).Result;

        PatchResponse PatchResponse = PatchTransaction(dbDataSource.OpenConnection(), dbDataSource, dateTimeProvider, offerId, user, data).Result;

        return new ValueTask<PatchResponse>(PatchResponse);
    }
}