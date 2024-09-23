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

public class Fields
{

    public struct GetData
    {
        public string id;
        public float square_meters;
        public string crop_type;
        public string irrigation_type;
    }

    public static List<GetData> GetFields(
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

        using DbConnection connection = dataSource.OpenConnection();

        using DbCommand commandGetFields = dataSource.CreateCommand();

        commandGetFields.CommandText = $@"
            select id, square_meters, crop_type, irrigation_type
            from farm_field
            where vat_number = $1
        ";

        commandGetFields.Parameters.Add(DbUtility.CreateParameter(connection, DbType.String, user.company_vat_number));
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
                square_meters = reader.GetFloat(1),
                crop_type = reader.GetString(2),
                irrigation_type = reader.GetString(3)
            });
        }
        reader.Close();
        connection.Close();
        return data;
    }
}