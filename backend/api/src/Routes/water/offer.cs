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

    public static List<GetData> GetWaterOffer(
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
            throw new AuthorizationException(AuthorizationException.ErrorCode.INVALID_AUTHORIZATION_HEADER, error_message);
        }
        if (_scheme != Authorization.Scheme.Bearer)
        {
            throw new AuthorizationException(AuthorizationException.ErrorCode.INVALID_AUTHORIZATION_HEADER, "Bearer scheme required");
        }
        Token token = Authentication.VerifiedPayload(_token, remoteManager, dateTimeProvider);
        User user = new User(
            global_id: token.sub,
            role: token.role,
            company_vat_number: token.company_vat_number
        );

        if (User.GetRole(user) != User.Role.FA)
        {
            throw new AuthorizationException(AuthorizationException.ErrorCode.INVALID_ROLE, "Only FA users can access this route");
        }

        //field_id is not passed in the headers, so we need to get it from the URL
        string field_id = headers["field_id"].Count > 0 ? headers["field_id"].ToString() : string.Empty;
        if (field_id == string.Empty)
        {
            throw new FieldException(FieldException.ErrorCode.FIELD_ID_REQUIRED, "Field id required");
        }

        using DbConnection connection = dataSource.OpenConnection();

        using DbCommand commandGetFields = dataSource.CreateCommand();

        commandGetFields.CommandText = $@"
            select id, price_liter, available_liters, publish_date
            from offer
            where publish_date >= CURRENT_DATE + INTERVAL '1 day'
        ";
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
                amount = reader.GetFloat(1),
                price = reader.GetFloat(2),
                date = reader.GetDateTime(3)
            });
        }
        reader.Close();
        connection.Close();
        return data;
    }
}

public class FieldException : Exception
{
    public enum ErrorCode
    {
        INVALID_FIELD_ID,
        FIELD_ID_REQUIRED
    }

    public ErrorCode Code { get; set; }

    public FieldException(ErrorCode code, string message) : base(message)
    {
        Code = code;
    }
}