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

    [Test]
    public static void TestCountSeconds() {
        List<Utility.CountEntity> entities = new List<Utility.CountEntity> {
            new Utility.CountEntity {
                date = DateTimeOffset.Parse("2022-01-01T00:00:00"),
                status = true
            },
            new Utility.CountEntity {
                date = DateTimeOffset.Parse("2022-01-01T00:00:10"),
                status = true
            },
            new Utility.CountEntity {
                date = DateTimeOffset.Parse("2022-01-01T00:00:20"),
                status = false
            },
            new Utility.CountEntity {
                date = DateTimeOffset.Parse("2022-01-01T00:00:30"),
                status = true
            },
            new Utility.CountEntity {
                date = DateTimeOffset.Parse("2022-01-01T00:00:40"),
                status = false
            }
        };
        Assert.AreEqual(
            Utility.CountSeconds(
                DateTimeOffset.Parse("2022-01-01T00:00:00").DateTime,
                entities
            ),
            30
        );

        entities = new List<Utility.CountEntity> {
            new Utility.CountEntity {
                date = new DateTimeOffset(new DateTime(2022, 1, 1, 23, 0, 0, DateTimeKind.Utc)),
                status = true
            },
        };
        Assert.AreEqual(
            Utility.CountSeconds(
                new DateTimeOffset(new DateTime(2022, 1, 2, 0, 0, 0, DateTimeKind.Utc)),
                entities
            ),
            (60*60*24)+3600
        );

        entities = new List<Utility.CountEntity> {
            new Utility.CountEntity {
                date = new DateTimeOffset(new DateTime(2022, 1, 1, 23, 0, 0, DateTimeKind.Utc)),
                status = true
            },
            new Utility.CountEntity {
                date = new DateTimeOffset(new DateTime(2022, 1, 2, 12, 00, 00, DateTimeKind.Utc)),
                status = true
            },
        };
        Assert.AreEqual(
            Utility.CountSeconds(
                new DateTimeOffset(new DateTime(2022, 1, 2, 0, 0, 0, DateTimeKind.Utc)),
                entities
            ),
            (60*60)+(60*60*24)
        );

        entities = new List<Utility.CountEntity> {
            new Utility.CountEntity {
                date = new DateTimeOffset(new DateTime(2022, 1, 1, 23, 0, 0, DateTimeKind.Utc)),
                status = true
            },
            new Utility.CountEntity {
                date = new DateTimeOffset(new DateTime(2022, 1, 2, 12, 00, 00, DateTimeKind.Utc)),
                status = false
            },
        };
        Assert.AreEqual(
            Utility.CountSeconds(
                new DateTimeOffset(new DateTime(2022, 1, 2, 0, 0, 0, DateTimeKind.Utc)),
                entities
            ),
            (60*60)+(60*60*12)
        );

        entities = new List<Utility.CountEntity> {
            new Utility.CountEntity {
                date = new DateTimeOffset(new DateTime(2022, 1, 1, 23, 0, 0, DateTimeKind.Utc)),
                status = false
            },
            new Utility.CountEntity {
                date = new DateTimeOffset(new DateTime(2022, 1, 2, 12, 00, 00, DateTimeKind.Utc)),
                status = true
            },
        };
        Assert.AreEqual(
            Utility.CountSeconds(
                new DateTimeOffset(new DateTime(2022, 1, 2, 0, 0, 0, DateTimeKind.Utc)),
                entities
            ),
            (60*60*12)
        );

        entities = new List<Utility.CountEntity> {
            new Utility.CountEntity {
                date = new DateTimeOffset(new DateTime(2022, 1, 2, 12, 00, 00, DateTimeKind.Utc)),
                status = true
            },
        };
        Assert.AreEqual(
            Utility.CountSeconds(
                new DateTimeOffset(new DateTime(2022, 1, 2, 0, 0, 0, DateTimeKind.Utc)),
                entities
            ),
            (60*60*12)
        );

        entities = new List<Utility.CountEntity> {
            new Utility.CountEntity {
                date = new DateTimeOffset(new DateTime(2022, 1, 2, 12, 00, 00, DateTimeKind.Utc)),
                status = false
            },
        };
        Assert.AreEqual(
            Utility.CountSeconds(
                new DateTimeOffset(new DateTime(2022, 1, 2, 0, 0, 0, DateTimeKind.Utc)),
                entities
            ),
            0
        );
    }
}