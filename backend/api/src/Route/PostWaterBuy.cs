using System.Collections.Specialized;
using System.Data.Common;
using System.Threading.Channels;

using Middleware;
using Module.JsonWebToken;
using Interface.Utility;

using System.Security.Claims;

namespace Route;

public class PostWaterBuy {
    private readonly DbDataSource _dbDataSource;
    private readonly IClockCustom _clock;
    private readonly JwtControl _jwtControl;

    public PostWaterBuy(
        DbDataSource dbDataSource,
        IClockCustom clock,
        JwtControl jwtControl
    ) {
        this._dbDataSource = dbDataSource;
        this._clock = clock;
        this._jwtControl = jwtControl;
    }
    /*public async Task<Tuple<uint, string>> HandleRoute(
        DbDataSource dbDataSource,
        NameValueCollection headers
    ) {
        bool isAuthenticated = Authentication.IsAuthenticated(headers["Authorization"], out string jwt, out string message);
        if(!isAuthenticated) return new Tuple<uint, string>(401, message);
        bool isValidToken = _jwtControl.GetClaims(jwt, out ClaimsPrincipal? principal, out message);
        if(!isValidToken) return new Tuple<uint, string>(401, message);
        bool isValidClaim = Authentication.CheckTokenClaim(principal, out message);
        if(!isValidClaim) return new Tuple<uint, string>(401, message);
        return new Tuple<uint, string>(200, "OK");
    }*/

}