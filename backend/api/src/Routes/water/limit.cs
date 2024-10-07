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


    /*
    
        In this Microservices setup, it is possible that a company that exists in authentication service does 
        not exist in this api service. This is because company record in database is created when the user
        issues a post request to create a field (see jwt access token payload for more information).
        Therefore, if a limit is set to a company that does not exists in this service yet, the following
        endpoint will return a 404 error (Not yet available). 

    */
    public static Task PostWaterLimit(
        IHeaderDictionary headers,
        Stream body,
        DbDataSource dataSource,
        IDateTimeProvider dateTimeProvider,
        RemoteManager remoteManager
    ) {
        User user = Authorization.AllowByRole(headers, remoteManager, dateTimeProvider, new List<User.Role> { User.Role.WA });
        
        PostWaterLimitBody.Request _body = JsonSerializer.DeserializeAsync<PostWaterLimitBody.Request>(body, new JsonSerializerOptions {
            PropertyNameCaseInsensitive = true,
            UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
        }).Result ?? throw new WaterLimitException(WaterLimitException.ErrorCode.INVALID_BODY, "Invalid body");
        _body.validate().Wait();
        
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