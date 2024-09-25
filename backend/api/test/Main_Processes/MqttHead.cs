using System;

namespace Main_Processes;
public static class TopicSchema_Test {
    [Test]
    public static void ParseTest() {
        TopicSchema? result = TopicSchema.Parse("/backend/measure/sensor/tmp");
        Assert.AreEqual(TopicSchema.Type.Temperature, result.type);
        result = TopicSchema.Parse("/backend/measure/sensor/umdty");
        Assert.AreEqual(TopicSchema.Type.Humidity, result.type);
        result = TopicSchema.Parse("/backend/measure/actuator");
        Assert.AreEqual(TopicSchema.Type.Actuator, result.type);
        result = TopicSchema.Parse("/backend/measure/sensor/tmp/other");
        Assert.IsNull(result);
        result = TopicSchema.Parse("/backend/measure/actuator/random");
        Assert.IsNull(result);
    }
}