using Moq;
using Module.KeyManager;

using Data;

namespace Module.Middleware;

public class AuthenticationTest {
    [Test]
    public void VerifyTest() {
        CustomKeyManager keyManager = new CustomKeyManager();
        var mockRemoteManager = new Mock<IRemoteJwksHub>();
        mockRemoteManager.Setup(x=>x.GetKey("key1", "www.example.com")).Returns(
            keyManager.GetPublicKey("key1")
        );
        string token = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCIsImtpZCI6ImtleTEifQ.eyJpc3MiOiJ3d3cuZXhhbXBsZS5jb20iLCJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiYWRtaW4iOnRydWUsImlhdCI6MTUxNjIzOTAyMn0.mEf0kfM1JzM3r6-RkyJkX6ZxDCD6Me3OTjU9w2DgAO0ltsvu43VqtsyZqYFuauaRiJY2FqYHVw2dmepbs0onANhC90B3N73PIj5XmMHt1aITfGzam_oqPVkKkJWQDzhOJZghjZHMJfeR1Xoa1TXdB4JxhS7qSgqw7jggjWL4H8BYCQHCpdG_q9OoRvxQBaM7B4pLXtWo8ED5ekT6KISVu6Jq2X7-zviq68rs-x4ISzWNK83o9w31OFnSbl2zwb7tml0-uOD1VGJrUTto2Qfog5cd-pao0ZnT4BIODuGF0OSutX_aHkHpuTE-bichFc3DOFfl4o7au-7KY_5eg98llQ";
        var result = Authentication.VerifySignature(
            mockRemoteManager.Object,
            token
        );
        Assert.AreEqual("www.example.com", result.iss);
    }
}