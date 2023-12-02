using Interface.Utility;
using Interface.Module.JsonWebToken;
using Data;
using System.Security.Claims;
using Moq;

namespace Module.JsonWebToken;

class PresignedToken
{
    //test edge cases with presigned jwt tokens using keys from KeyManager.cs
    public string getToken1()
    {
        // iat: 1704070800, exp: 1704080800, kid: key1, sub: 1234567890, roles: [FAR]
        return @"eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCIsImtpZCI6ImtleTEifQ.eyJzdWIiOiIxMjM0NTY3ODkwIiwicm9sZXMiOlsiRkFSIl0sImlhdCI6MTcwNDA2NzIwMCwiZXhwIjoxNzA0MDgwODAwfQ.rjm8GEuLzJOyMBHSOqreNC3BF5-z2u2jRoRB02ha2asubKOMz_pDVcXdiub7w33LkBorWaotVSDFg_f7mIjg7GEscQgLEzJ5ofmyBpmba3gqupUIdf4F1-K-1aaaH1Ult8m9NLxe-asdj1qY8It6okQz085ULwm6K3wgcb7WGNDyE1emE16k5_SCCdAXCR3D8fO0KphQvT02vYmfyTkS6I2_LGRENWi1HDjhbDz05vz3DlTMWr_hzYNwpvYGPAoARsmpUnFLkWTvTM8cjBfMDbVe7h8JwWDd-wSzPuBc8su6ZUxLwLChlhJWnN7Q15v9LCUi54GXI832NvCg-StFqQ";
    }

    public string getToken2()
    {
        // iat: 1704070800, exp: 1704080800, kid: random, sub: 1234567890, roles: [FAR]
        return @"eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCIsImtpZCI6InJhbmRvbSJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwicm9sZXMiOlsiRkFSIl0sImlhdCI6MTcwNDA3MDgwMCwiZXhwIjoxNzA0MDgwODAwfQ.Qg3EjJDHDV5Ls1JZ2ONpdcZHV05r4d6aZbwn7TA-wB-pv4oqXbIcLe6jzCbSMGk3ZjIgCiuy0ceINBxIU-b44VDNMkrB9r09G98mM8TP02dwZsKc0151f9kMizN2T7zk8W-GTG9I9LAeCgNKt1CAFegsXCt_qjnr4gp1B9SB4-lVW8A3Q3WyogxBR02msIXQk2lxvdPcyud3B4EknQ0oCcz5ejabQh2DpDIsP0HyLE3CS8jqOTBJb2bbNeS5OZ5-dG32uLLd6LOLaS-NcuoYKxJFPCyRL0tdwqUIpbvlaA3px0vwWzTyjN2oElqAHCXnvOr_9xEAoDVe4fdbSREEZQ";
    }
}

public class JwtControlTest
{

    [Test]
    public async Task TestUpdateKeys()
    {
        PresignedToken presignedToken = new PresignedToken();
        KeyManager keyManager = new KeyManager();
        var clockMock = new Mock<IClockCustom>();
        var fetchMock = new Mock<IFetch>();
        var keyStoreMock = new Mock<IJwtKeyStore>();
        keyStoreMock.Setup(x => x.isExpired()).Returns(false);
        keyStoreMock.Setup(x => x.GetKey("key1")).Returns(
            keyManager.GetPublicKey("key1")
        );
        JwtControl jwtControl = new JwtControl(
            keyStoreMock.Object,
            clockMock.Object,
            fetchMock.Object,
            "https://mybackend.example.com/api"
        );

        {
            clockMock.Setup(x => x.UtcNow()).Returns(DateTimeOffset.FromUnixTimeSeconds(1704080700).DateTime);
            (bool isOk, ClaimsPrincipal principal) = await jwtControl.GetClaims(presignedToken.getToken1());
            Assert.IsTrue(isOk);
            Assert.IsNotNull(principal);
        }

        {
            clockMock.Setup(x => x.UtcNow()).Returns(DateTimeOffset.FromUnixTimeSeconds(1704080700).DateTime);
            (bool isOk, ClaimsPrincipal principal) = await jwtControl.GetClaims(presignedToken.getToken2());
            Assert.IsFalse(isOk);
            Assert.IsNull(principal);
        }

        {
            clockMock.Setup(x => x.UtcNow()).Returns(DateTimeOffset.FromUnixTimeSeconds(1704080801).DateTime);
            (bool isOk, ClaimsPrincipal principal) = await jwtControl.GetClaims(presignedToken.getToken1());
            Assert.IsFalse(isOk);
            Assert.IsNull(principal);
        }

    }

}