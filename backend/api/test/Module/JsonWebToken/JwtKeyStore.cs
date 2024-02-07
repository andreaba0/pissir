using Moq;
using Interface.Module.JsonWebToken;
using Data;
using Interface.Utility;

namespace Module.JsonWebToken;

public class JwtKeyStoreTest {

    [Test]
    public void KeyExpirationTest() {
        var clockMock = new Mock<IClockCustom>();
        JwtKeyStore store = new JwtKeyStore(clockMock.Object);
        KeyManager keyManager = new KeyManager();
        clockMock.Setup(x => x.Now()).Returns(DateTime.Now);
        Assert.IsTrue(store.isExpired());
        store.SetKey("key1", keyManager.GetPublicKey("key1"));
        store.setExpiration(3600);
        Assert.IsFalse(store.isExpired());
        clockMock.Setup(x => x.Now()).Returns(DateTime.Now.AddSeconds(3601));
        Assert.IsTrue(store.isExpired());
        Assert.IsNull(store.GetKey("key1"));
    }

}