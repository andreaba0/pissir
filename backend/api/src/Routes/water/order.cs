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

namespace Routes;

public class WaterOrderRoute
{
    public struct GetData
    {
        public string offer_id;
        public string company_vat_number;
        public string field_id;
        public float quantity;
    }

    public static List<GetData> GetSensorData(
        IHeaderDictionary headers,
        DbDataSource dataSource,
        IDateTimeProvider dateTimeProvider,
        RemoteManager remoteManager
    )
    {
        bool ok = false;
        string bearer_token = headers["Authorization"].Count > 0 ? headers["Authorization"].ToString() : string.Empty;
        ok = Authorization.tryParseAuthorizationHeader(bearer_token, out Authorization.Scheme _scheme, out string _token, out string error_message);
        if (!ok)
        {
            throw new WaterLimitException(WaterLimitException.ErrorCode.INVALID_AUTHORIZATION_HEADER, error_message);
        }
        if (_scheme != Authorization.Scheme.Bearer)
        {
            throw new WaterLimitException(WaterLimitException.ErrorCode.INVALID_AUTHORIZATION_HEADER, "Bearer scheme required");
        }
        Token token = Authentication.VerifiedPayload(_token, remoteManager, dateTimeProvider);
        User user = new User(
            global_id: token.sub,
            role: token.role,
            company_vat_number: token.company_vat_number
        );

        using DbConnection connection = dataSource.OpenConnection();

        using DbCommand commandGetSensorData = dataSource.CreateCommand();


        if (User.GetRole(user) == User.Role.FA)
        {
            /*Get a list of all buy orders for each field of the farm belonging to the user*/
            commandGetSensorData.CommandText = $@"
                select offer_id, company_vat_number, field_id, quantity
                from buy_order inner join offer on buy_order.offer_id = offer.id
                inner join farm_field on buy_order.farm_field_id = farm_field.id
                where farm_field.vat_number = $1
            ";
        } else {
            /*Get a list of all the buy orders made by each farm company that belong to the company user works for*/
            commandGetSensorData.CommandText = $@"
                select offer_id, company_vat_number, field_id, quantity
                from buy_order inner join offer on buy_order.offer_id = offer.id
                where offer.vat_number = $1
            ";
        }
        commandGetSensorData.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, user.company_vat_number));

        using DbDataReader reader = commandGetSensorData.ExecuteReader();

        if (!reader.HasRows)
        {
            return new List<GetData>();
        }

        List<GetData> data = new List<GetData>();

        while (reader.Read())
        {
            data.Add(new GetData
            {
                offer_id = reader.GetString(0),
                company_vat_number = reader.GetString(1),
                field_id = reader.GetString(2),
                quantity = reader.GetFloat(3)
            });
        }
        reader.Close();
        connection.Close();
        return data;
    }
}