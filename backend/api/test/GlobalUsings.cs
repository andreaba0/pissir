global using NUnit.Framework;

DateTime now = DateTime.Now;
Console.WriteLine(now);
// get timezone offset
TimeSpan offset = TimeZoneInfo.Local.GetUtcOffset(now);
Console.WriteLine(offset);
// get time in UTC
DateTime utcNow = DateTime.UtcNow;
Console.WriteLine(utcNow);

DateTime custom = new DateTime(2021, 10, 21, 23, 59, 59);
// get datetime now
Console.WriteLine(custom);

// get timezone offset
TimeSpan offsetCustom = TimeZoneInfo.Local.GetUtcOffset(custom);
Console.WriteLine(offsetCustom);