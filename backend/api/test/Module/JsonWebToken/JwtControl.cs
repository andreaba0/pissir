using Interface.Utility;
using Interface.Module.JsonWebToken;
using Data;
using System.Security.Claims;
using Moq;
using Middleware;

namespace Module.JsonWebToken;

class PresignedToken
{
    //test edge cases with presigned jwt tokens using keys from KeyManager.cs
    public string getToken1()
    {
        // iat: 1704070800, exp: 1704080800, kid: key1, sub: 1234567890, roles: [FAR]
        //return @"eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCIsImtpZCI6ImtleTEifQ.eyJzdWIiOiIxMjM0NTY3ODkwIiwicm9sZXMiOlsiRkFSIl0sImlhdCI6MTcwNDA2NzIwMCwiZXhwIjoxNzA0MDgwODAwfQ.rjm8GEuLzJOyMBHSOqreNC3BF5-z2u2jRoRB02ha2asubKOMz_pDVcXdiub7w33LkBorWaotVSDFg_f7mIjg7GEscQgLEzJ5ofmyBpmba3gqupUIdf4F1-K-1aaaH1Ult8m9NLxe-asdj1qY8It6okQz085ULwm6K3wgcb7WGNDyE1emE16k5_SCCdAXCR3D8fO0KphQvT02vYmfyTkS6I2_LGRENWi1HDjhbDz05vz3DlTMWr_hzYNwpvYGPAoARsmpUnFLkWTvTM8cjBfMDbVe7h8JwWDd-wSzPuBc8su6ZUxLwLChlhJWnN7Q15v9LCUi54GXI832NvCg-StFqQ";
        return @"eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCIsImtpZCI6ImtleTEifQ.eyJzdWIiOiIxMjM0NTY3ODkwIiwicm9sZXMiOlsiRkFSIl0sImlhdCI6MTcwNDA2NzIwMCwiZXhwIjoxNzA0MDgwODAwLCJuYmYiOjE3MDQwNjcyMDB9.LqmffZ0fuzGzO7793aEjAidIqYCDBHkTG0tofsQd0IKrLSM9LJs5wF9JOOi685x6lAQ8GItqqHFpcAR71eien2vF9FLAKQ6RK-C3EKG_GQ43FUKSivu2fiu6jTpUervmz8I2S8wHbNbm58AKSfw7YWScavjDAd73m9bcedgXxiVR4yO3VzpWStWCskBjJc5QtHr74Nh9CZr9nCfPPc6j4PT-UgtZCnmr0LY21seZCvrCpOJ7HNbD0DgkT8v1bhwDwKRbQ0RP_Co5DNLq2qugKaNcJCh5I3rpjF0mg8KpliB8PZRfmgxiyT6srdENedaHUb_4Jsx6302LDpUp1pWJxw";
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
    public async Task UpdateKeysTest()
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
            bool isOk = jwtControl.GetClaims(presignedToken.getToken1(), out ClaimsPrincipal principal, out string message);
            Assert.IsTrue(isOk);
            Assert.IsNotNull(principal);
            Assert.That(message, Is.Empty);
        }

        {
            clockMock.Setup(x => x.UtcNow()).Returns(DateTimeOffset.FromUnixTimeSeconds(1704080700).DateTime);
            bool isOk = jwtControl.GetClaims(presignedToken.getToken2(), out ClaimsPrincipal principal, out string message);
            Assert.IsFalse(isOk);
            Assert.IsNull(principal);
            Assert.That(message, Is.EqualTo("Key not found"));
        }

        {
            clockMock.Setup(x => x.UtcNow()).Returns(DateTimeOffset.FromUnixTimeSeconds(1704080801).DateTime);
            bool isOk = jwtControl.GetClaims(presignedToken.getToken1(), out ClaimsPrincipal principal, out string message);
            Assert.IsFalse(isOk);
            Assert.IsNull(principal);
            Assert.That(message, Is.EqualTo("Token expired"));
        }

        {
            clockMock.Setup(x => x.UtcNow()).Returns(DateTimeOffset.FromUnixTimeSeconds(1704080700).DateTime);
            bool isOk = jwtControl.GetClaims(presignedToken.getToken1(), out ClaimsPrincipal principal, out string message);
            //bool isOk2 = Authentication.CheckTokenClaim(principal, out string message);
            Assert.IsTrue(isOk);
        }

    }

}