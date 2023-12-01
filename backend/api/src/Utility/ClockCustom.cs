using Interface.Utility;

namespace Utility;

public class ClockCustom : IClockCustom {
    public DateTime Now() {
        return DateTime.Now;
    }

    public DateTime UtcNow() {
        return DateTime.UtcNow;
    }
}