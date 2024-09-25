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
    public DateTimeProvider ParseStartTime(string startTime) {
        Regex regex = new Regex(@"^(\d{4})-(\d{2})-(\d{2})T(\d{2}):(\d{2}):(\d{2})$");
        Match match = regex.Match(startTime);
        if (match.Success) {
            int year = int.Parse(match.Groups[1].Value);
            int month = int.Parse(match.Groups[2].Value);
            int day = int.Parse(match.Groups[3].Value);
            int hour = int.Parse(match.Groups[4].Value);
            int minute = int.Parse(match.Groups[5].Value);
            int second = int.Parse(match.Groups[6].Value);
            //datetime should be in GMT+2
            DateTime start = new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc);
            DateTimeOffset now = DateTimeOffset.Now;
            DateTimeOffset startOffset = new DateTimeOffset(start);
            offsetSeconds = (long) now.ToUnixTimeSeconds() - (long) startOffset.ToUnixTimeSeconds();
            return this;
        }
        else {
            throw new ArgumentException("Invalid start time format");
        }
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