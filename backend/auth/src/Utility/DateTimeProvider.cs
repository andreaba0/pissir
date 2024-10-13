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
    public static DateTimeProvider parse(string baseDate)
    {
        Regex regex = new Regex(@"^(\d{2})-(\d{2})-(\d{4})T(\d{2}):(\d{2}):(\d{2})$");
        Match match = regex.Match(baseDate);
        if (match.Success)
        {
            int year = int.Parse(match.Groups[3].Value);
            int month = int.Parse(match.Groups[2].Value);
            int day = int.Parse(match.Groups[1].Value);
            int hour = int.Parse(match.Groups[4].Value);
            int minute = int.Parse(match.Groups[5].Value);
            int second = int.Parse(match.Groups[6].Value);

            // parameter is loaded as local timezone
            DateTime start = new DateTime(year, month, day, hour, minute, second, DateTimeKind.Local);

            return new DateTimeProvider(start);
        }
        else
        {
            throw new ArgumentException("Invalid start time format");
        }
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
    /*public DateTimeProvider(DateTime startTime)
    {
        DateTimeOffset now = DateTimeOffset.Now;
        DateTimeOffset start = new DateTimeOffset(startTime);
        offsetSeconds = (long) now.ToUnixTimeSeconds() - (long) start.ToUnixTimeSeconds();
    }*/
    public DateTimeProvider(DateTime startTime)
    {
        // This create a DateTime object and the timezone is supposed to be Europe/Rome
        DateTime initial = new DateTime(startTime.Year, startTime.Month, startTime.Day, startTime.Hour, startTime.Minute, startTime.Second, startTime.Kind);

        // With initial DateTime it is necessary to calculate the offset between its utc time and the actual utc time
        long epoch_initial = epoch(initial);
        long epoch_now = epoch(DateTime.UtcNow);
        offsetSeconds = epoch_initial - epoch_now;
    }

    //public DateTime Now => DateTime.Now.AddSeconds(offsetSeconds*(-1));
    //public DateTime UtcNow => DateTime.UtcNow.AddSeconds(offsetSeconds*(-1));
    public DateTime Now => DateTime.Now.AddSeconds(offsetSeconds);
    public DateTime UtcNow => DateTime.UtcNow.AddSeconds(offsetSeconds);
    public DateTime FromUnixTime(long unixTime)
    {
        DateTimeOffset start = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
        DateTimeOffset now = DateTimeOffset.Now;
        DateTimeOffset offset = start.AddSeconds(unixTime);
        return offset.AddSeconds(offsetSeconds*(-1)).DateTime;
    }
    public static long epoch(DateTime dt)
    {
        DateTime utcTime = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, DateTimeKind.Utc);
        if (dt.Kind == DateTimeKind.Local || dt.Kind == DateTimeKind.Unspecified)
        {
            DateTime dtNew = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, DateTimeKind.Unspecified);
            string timeZoneName = "Europe/Rome";
            TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneName);
            utcTime = TimeZoneInfo.ConvertTimeToUtc(dtNew, timeZone);
        }
        DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        TimeSpan epochTimeSpan = utcTime - epoch;
        return (long)epochTimeSpan.TotalSeconds;
    }
}