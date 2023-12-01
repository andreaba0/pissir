using Interface.Utility;
using Interface.Module.JsonWebToken;
using Data;
using System.Security.Claims;
using Moq;

namespace Module.JsonWebToken;

class PresignedToken {
    public string getToken1() {
        // iat: 1704070800, exp: 1704080800, kid: key1, sub: 1234567890, roles: [FAR]
        return @"eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCIsImtpZCI6ImtleTEifQ.eyJzdWIiOiIxMjM0NTY3ODkwIiwicm9sZXMiOlsiRkFSIl0sImlhdCI6MTcwNDA2NzIwMCwiZXhwIjoxNzA0MDgwODAwfQ.rjm8GEuLzJOyMBHSOqreNC3BF5-z2u2jRoRB02ha2asubKOMz_pDVcXdiub7w33LkBorWaotVSDFg_f7mIjg7GEscQgLEzJ5ofmyBpmba3gqupUIdf4F1-K-1aaaH1Ult8m9NLxe-asdj1qY8It6okQz085ULwm6K3wgcb7WGNDyE1emE16k5_SCCdAXCR3D8fO0KphQvT02vYmfyTkS6I2_LGRENWi1HDjhbDz05vz3DlTMWr_hzYNwpvYGPAoARsmpUnFLkWTvTM8cjBfMDbVe7h8JwWDd-wSzPuBc8su6ZUxLwLChlhJWnN7Q15v9LCUi54GXI832NvCg-StFqQ";
    }
}

public class JwtControlTest {

    [Test]
    public async Task TestUpdateKeys() {
        PresignedToken presignedToken = new PresignedToken();
        KeyManager keyManager = new KeyManager();
        var clockMock = new Mock<IClockCustom>();
        clockMock.Setup(x=>x.UtcNow()).Returns(DateTimeOffset.FromUnixTimeSeconds(1704070800).DateTime);
        var fetchMock = new Mock<IFetch>();
        var keyStoreMock = new Mock<IJwtKeyStore>();
        keyStoreMock.Setup(x=>x.isExpired()).Returns(false);
        keyStoreMock.Setup(x=>x.GetKey("key1")).Returns(
            keyManager.GetPublicKey("key1")
        );
        JwtControl jwtControl = new JwtControl(
            keyStoreMock.Object,
            clockMock.Object,
            fetchMock.Object,
            "https://mybackend.example.com/api"
        );

        ClaimsPrincipal principal = await jwtControl.GetClaims(presignedToken.getToken1());

        clockMock.Setup(x=>x.UtcNow()).Returns(DateTimeOffset.FromUnixTimeSeconds(1704080801).DateTime);
        Assert.IsNull(await jwtControl.GetClaims(presignedToken.getToken1()));


    }

}