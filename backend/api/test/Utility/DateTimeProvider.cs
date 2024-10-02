using System;

namespace Utility;
public static class DateTimeProviderTest {
    [Test]
    public static void TestNowAndUtcNow() {
        /*IDateTimeProvider dtp = DateTimeProvider.parse("01-05-2010T04:00:00");
        DateTime now = dtp.Now;
        DateTime utcNow = dtp.UtcNow;
        long epochNow = DateTimeProvider.epoch(now);
        Console.WriteLine($"Now: {now.Kind}, {now}");
        Console.WriteLine($"UtcNow: {utcNow.Kind}, {utcNow}");
        long epochUtcNow = DateTimeProvider.epoch(utcNow);
        TimeZoneInfo localZone = TimeZoneInfo.Local;
        TimeSpan offset = localZone.BaseUtcOffset;
        long zoneSeconds = (long) offset.TotalSeconds;
        Console.WriteLine(offset);
        Console.WriteLine(localZone);
        bool isAround = (epochNow + zoneSeconds >= epochUtcNow - 10) && (epochNow + zoneSeconds <= epochUtcNow + 10);
        Console.WriteLine(epochNow);
        Console.WriteLine(epochUtcNow);
        Console.WriteLine(zoneSeconds);
        Console.WriteLine(TimeSpan.Zero);
        Assert.IsTrue(isAround);*/
    }

    [Test]
    public static void TestEpoch() {
        // This test is done to check if epoch function works for both CE(Summer) and CET(Winter) timezones

        long epoch1 = DateTimeProvider.epoch(new DateTime(2024, 10, 2, 21, 15, 0, DateTimeKind.Local));
        Assert.AreEqual(1727896500, epoch1);

        long epoch2 = DateTimeProvider.epoch(new DateTime(2024, 1, 2, 21, 15, 0, DateTimeKind.Local));
        Assert.AreEqual(1704226500, epoch2);
    }
}