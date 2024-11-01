namespace Types;

public class HumiditySensor {
    public float value { get; set; }

    public HumiditySensor(float value) {
        this.value = value;
    }
}

public class TemperatureSensor {
    public float value { get; set; }

    public TemperatureSensor(float value) {
        this.value = value;
    }
}