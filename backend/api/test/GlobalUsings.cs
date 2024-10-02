global using NUnit.Framework;

/*DateTime now = DateTime.Now;
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
Console.WriteLine(offsetCustom);*/

foreach (TimeZoneInfo tzl in TimeZoneInfo.GetSystemTimeZones()) {
    Console.WriteLine(tzl.Id);
}

DateTime now = new DateTime(2024, 1, 6, 14, 0, 0);

string timeZoneName = "Europe/Rome";
TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneName);
//DateTime customTime = TimeZoneInfo.ConvertTime(now, timeZone, TimeZoneInfo.Local);
//Console.WriteLine(now.Now);

Console.Write("Time in " + timeZoneName + ": ");
Console.WriteLine(now);

DateTime utcTime = TimeZoneInfo.ConvertTimeToUtc(now, timeZone);
Console.Write("Time in UTC: ");
Console.WriteLine(utcTime);
//print utcTime kind
Console.WriteLine(utcTime.Kind);

Console.WriteLine(timeZone.IsDaylightSavingTime(now));

//calculate epoch from utcTime
DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
TimeSpan epochTimeSpan = utcTime - epoch;
long epochTime = (long)epochTimeSpan.TotalSeconds;
Console.WriteLine(epochTime);