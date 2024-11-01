namespace Types;

public class Actuator {
    public string status { get; set; }
    public long period { get; set; }
    public float water_used { get; set; }

    public Actuator(string status, int period, float water_used) {
        this.status = status;
        this.period = period;
        this.water_used = water_used;
    }
}