using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Utility;

public interface IDateTimeProvider
{
    DateTime Now { get; }
    DateTime FromUnixTime(long unixTime);
    DateTime UtcNow { get; }
}

public class DateTimeProvider : IDateTimeProvider
{
    private long offsetSeconds = 0;
    public DateTimeProvider()
    {
        offsetSeconds = 0;
    }

    public static DateTimeProvider parse(string baseDate) {
        Regex regex = new Regex(@"^(\d{2})-(\d{2})-(\d{4})T(\d{2}):(\d{2}):(\d{2})$");
        Match match = regex.Match(baseDate);
        if (match.Success) {
            int year = int.Parse(match.Groups[3].Value);
            int month = int.Parse(match.Groups[2].Value);
            int day = int.Parse(match.Groups[1].Value);
            int hour = int.Parse(match.Groups[4].Value);
            int minute = int.Parse(match.Groups[5].Value);
            int second = int.Parse(match.Groups[6].Value);
            //datetime should be in GMT+2
            DateTime start = new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc);
            DateTimeOffset now = DateTimeOffset.Now;
            DateTimeOffset startOffset = new DateTimeOffset(start);

            //get offset based on time zone
            TimeZone localZone = TimeZone.CurrentTimeZone;
            TimeSpan localOffset = localZone.GetUtcOffset(DateTime.Now);
            int offset = localOffset.Hours * 3600 + localOffset.Minutes * 60;

            long offsetSeconds = (long) now.ToUnixTimeSeconds() - (long) startOffset.ToUnixTimeSeconds() + offset;

            //long offsetSeconds = (long) now.ToUnixTimeSeconds() - (long) startOffset.ToUnixTimeSeconds();
            return new DateTimeProvider(offsetSeconds);
        }
        else {
            throw new ArgumentException("Invalid start time format");
        }
    }

    public DateTimeProvider(long offsetSeconds)
    {
        this.offsetSeconds = offsetSeconds;
    }

    public DateTimeProvider(DateTime startTime)
    {
        DateTimeOffset now = DateTimeOffset.Now;
        DateTimeOffset start = new DateTimeOffset(startTime);
        offsetSeconds = (long) now.ToUnixTimeSeconds() - (long) start.ToUnixTimeSeconds();
    }
    public DateTime Now => DateTime.Now.AddSeconds(offsetSeconds*(-1));
    public DateTime UtcNow => DateTime.UtcNow.AddSeconds(offsetSeconds*(-1));
    public DateTime FromUnixTime(long unixTime)
    {
        DateTimeOffset start = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
        DateTimeOffset now = DateTimeOffset.Now;
        DateTimeOffset offset = start.AddSeconds(unixTime);
        return offset.AddSeconds(offsetSeconds*(-1)).DateTime;
    }
}