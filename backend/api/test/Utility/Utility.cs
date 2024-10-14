using System;

namespace Utility;
public static class UtilityTestClass {
    [Test]
    public static void TestHmacConverter() {
        Assert.AreEqual(
            Utility.HmacSha256("key123", "This is just a test"), 
            "56c4e621951f41aae205a2635b9627757bc4a00b297bd3d58bf992d8eb252570"
        );
        Assert.AreEqual(
            Utility.HmacSha256("key456", "A second test to make sure it works"),
            "209b145be5778c1965164ba611f55c997386ff52c6df3c5a3389b567c58ce202"
        );
    }

    [Test]
    public static void TestBase64URLDecode() {
        Assert.AreEqual(
            Utility.Base64URLDecode("VGhpcyBpcyBhIHRlc3Q"),
            "This is a test"
        );
        Assert.AreEqual(
            Utility.Base64URLDecode("QSBzZWNvbmQgdGVzdCB0byBtYWtlIHN1cmUgaXQgd29ya3M"),
            "A second test to make sure it works"
        );
    }
}