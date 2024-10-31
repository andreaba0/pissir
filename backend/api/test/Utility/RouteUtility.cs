using System;

namespace Utility;
public static class WaterLimitGroupTest {

    [Test]
    public static void GroupDataTest() {
        List<WaterLimitGroup.Record> data = new List<WaterLimitGroup.Record> {
            new WaterLimitGroup.Record("1", 1, new DateTime(2021, 1, 1)),
            new WaterLimitGroup.Record("1", 1, new DateTime(2021, 1, 2)),
            new WaterLimitGroup.Record("1", 1, new DateTime(2021, 1, 3)),
            new WaterLimitGroup.Record("1", 2, new DateTime(2021, 1, 4)),
            new WaterLimitGroup.Record("1", 2, new DateTime(2021, 1, 5)),
            new WaterLimitGroup.Record("1", 2, new DateTime(2021, 1, 6)),
            new WaterLimitGroup.Record("2", 1, new DateTime(2021, 1, 1)),
            new WaterLimitGroup.Record("2", 1, new DateTime(2021, 1, 2)),
            new WaterLimitGroup.Record("2", 2, new DateTime(2021, 1, 3)),
            new WaterLimitGroup.Record("2", 2, new DateTime(2021, 1, 4)),
            new WaterLimitGroup.Record("2", 2, new DateTime(2021, 1, 5)),
            new WaterLimitGroup.Record("2", 2, new DateTime(2021, 1, 6)),
        };
        List<WaterLimitGroup.GroupedData> groupedData = WaterLimitGroup.GroupData(data);
        Assert.That(groupedData.Count, Is.EqualTo(4));
        Assert.That(groupedData[0].company_vat_number, Is.EqualTo("1"));
        Assert.That(groupedData[0].limit, Is.EqualTo(1));
        Assert.That(groupedData[0].start_date, Is.EqualTo(new DateTime(2021, 1, 1)));
        Assert.That(groupedData[0].end_date, Is.EqualTo(new DateTime(2021, 1, 3)));
        Assert.That(groupedData[1].company_vat_number, Is.EqualTo("1"));
        Assert.That(groupedData[1].limit, Is.EqualTo(2));
        Assert.That(groupedData[1].start_date, Is.EqualTo(new DateTime(2021, 1, 4)));
        Assert.That(groupedData[1].end_date, Is.EqualTo(new DateTime(2021, 1, 6)));
    }
}