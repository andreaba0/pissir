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

public class WaterLimit {
    public static class PostWaterLimitBody {
        public class Request {
            public string vat_number { get; set; }
            public float limit { get; set; }
            public string start_date { get; set; }
            public string end_date { get; set; }

            public Task validate() {
                if (vat_number == null || vat_number == string.Empty) {
                    throw new WaterLimitException(WaterLimitException.ErrorCode.INVALID_BODY, "Vat number required");
                }
                if (limit <= 0) {
                    throw new WaterLimitException(WaterLimitException.ErrorCode.INVALID_BODY, "Limit must be greater than 0");
                }
                if (start_date == null || start_date == string.Empty) {
                    throw new WaterLimitException(WaterLimitException.ErrorCode.INVALID_BODY, "Start date required");
                }
                if (end_date == null || end_date == string.Empty) {
                    throw new WaterLimitException(WaterLimitException.ErrorCode.INVALID_BODY, "End date required");
                }
                return Task.CompletedTask;
            }
        }
    }

    public static Task PostWaterLimit(
        IHeaderDictionary headers,
        Stream body,
        DbDataSource dataSource,
        IDateTimeProvider dateTimeProvider,
        RemoteManager remoteManager
    ) {
        bool ok = false;
        string bearer_token = headers["Authorization"].Count > 0 ? headers["Authorization"].ToString() : string.Empty;
        ok = Authorization.tryParseAuthorizationHeader(bearer_token, out Authorization.Scheme _scheme, out string _token, out string error_message);
        if(!ok) {
            throw new WaterLimitException(WaterLimitException.ErrorCode.INVALID_AUTHORIZATION_HEADER, error_message);
        }
        if (_scheme != Authorization.Scheme.Bearer) {
            throw new WaterLimitException(WaterLimitException.ErrorCode.INVALID_AUTHORIZATION_HEADER, "Bearer scheme required");
        }
        Token token = Authentication.VerifiedPayload(_token, remoteManager, dateTimeProvider);
        User user = new User(
            global_id: token.sub,
            role: token.role,
            company_vat_number: token.company_vat_number
        );
        PostWaterLimitBody.Request _body = JsonSerializer.DeserializeAsync<PostWaterLimitBody.Request>(body, new JsonSerializerOptions {
            PropertyNameCaseInsensitive = true,
            UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
        }).Result ?? throw new WaterLimitException(WaterLimitException.ErrorCode.INVALID_BODY, "Invalid body");
        _body.validate().Wait();
        Console.WriteLine(_body.start_date);
        return Task.CompletedTask;
    }
}

public class WaterLimitException : Exception {
    public enum ErrorCode {
        INVALID_AUTHORIZATION_HEADER,
        INVALID_BODY
    }

    public ErrorCode code { get; private set; }
    public WaterLimitException(ErrorCode code, string message) : base(message) {
        this.code = code;
    }
}