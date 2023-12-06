namespace Route;

public static class PostWaterBuy {
    public async Task<Tuple<uint, string>> HandleRoute(
        DbDataSource dbDataSource,
        NameValueCollection headers
    ) {
        bool isAuthenticated = Authentication.IsAuthenticated(headers["Authorization"], out string jwt, out string message);
        if(!isAuthenticated) return new Tuple<uint, string>(401, message);
        bool isValidToken = JwtControl.GetClaims(jwt, out ClaimsPrincipal? principal, out message);
        if(!isValidToken) return new Tuple<uint, string>(401, message);
        bool isValidClaim = Authentication.CheckTokenClaim(principal, out message);
        if(!isValidClaim) return new Tuple<uint, string>(401, message);
        return new Tuple<bool, string>(200, "OK");
    }

}