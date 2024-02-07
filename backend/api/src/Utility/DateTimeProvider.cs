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