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

    /// <summary>
    /// This method parses a datetime string from the frontend to a DateTime object.
    /// </summary>
    /// <param name="datetime"></param>
    /// <returns></returns>
    public static DateTime parseDateFromFrontend(string datetime)
    {
        Regex regex = new Regex(@"^(\d{4})-(\d{2})-(\d{2})");
        Match match = regex.Match(datetime);
        if (!match.Success)
        {
            throw new ArgumentException("Invalid start time format");
        }
        int year = int.Parse(match.Groups[1].Value);
        int month = int.Parse(match.Groups[2].Value);
        int day = int.Parse(match.Groups[3].Value);
        return new DateTime(year, month, day);
    }

    //Parsing is always done in local timezone.
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

    public DateTimeProvider(DateTime startTime)
    {
        // This create a DateTime object and the timezone is supposed to be Europe/Rome
        DateTime initial = new DateTime(startTime.Year, startTime.Month, startTime.Day, startTime.Hour, startTime.Minute, startTime.Second, startTime.Kind);

        // With initial DateTime it is necessary to calculate the offset between its utc time and the actual utc time
        long epoch_initial = epoch(initial);
        long epoch_now = epoch(DateTime.UtcNow);
        offsetSeconds = epoch_initial - epoch_now;
    }
    public DateTime Now => DateTime.Now.AddSeconds(offsetSeconds);
    public DateTime UtcNow => DateTime.UtcNow.AddSeconds(offsetSeconds);
    public DateTime FromUnixTime(long unixTime)
    {
        DateTimeOffset start = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
        DateTimeOffset now = DateTimeOffset.Now;
        DateTimeOffset offset = start.AddSeconds(unixTime);
        return offset.AddSeconds(offsetSeconds * (-1)).DateTime;
    }

    /// <summary>
    /// Convert a DateTime object to epoch time
    /// </summary>
    /// <param name="dt">DateTime in UTC</param>
    /// <returns></returns>
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