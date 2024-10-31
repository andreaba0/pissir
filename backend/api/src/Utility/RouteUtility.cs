namespace Utility;

public class WaterLimitGroup {
    public class Record {
        public string vat_number { get; set; }
        public float limit { get; set; }
        public DateTime on_date { get; set; }

        public Record(string vat_number, float limit, DateTime on_date) {
            this.vat_number = vat_number;
            this.limit = limit;
            this.on_date = on_date;
        }
    }

    public class GroupedData {
        public string company_vat_number { get; set; }
        public float limit { get; set; }
        public DateTime start_date { get; set; }
        public DateTime end_date { get; set; }
    }

    /// <summary>
    /// This method groups data by company vat number, and limit by consecutive dates.
    /// Takes as input a list of limits for each company, ordered by date.
    /// </summary>
    /// <param name="data">A list of records ordered by date</param>
    /// <returns></returns>
    public static List<GroupedData> GroupData(List<Record> data) {
        List<GroupedData> groupedData = new List<GroupedData>();
        if (data.Count == 0) {
            return groupedData;
        }
        Dictionary<string, List<GroupedData>> groups = new Dictionary<string, List<GroupedData>>();
        for (int i = 0; i < data.Count; i++) {
            Record record = data[i];
            string vatNumber = record.vat_number;
            if (!groups.ContainsKey(vatNumber)) {
                groups[vatNumber] = new List<GroupedData>();
            }
            List<GroupedData> currentGroup = groups[vatNumber];
            if(currentGroup.Count == 0) {
                currentGroup.Add(new GroupedData {
                    company_vat_number = vatNumber,
                    limit = record.limit,
                    start_date = record.on_date,
                    end_date = record.on_date
                });
                continue;
            }
            GroupedData lastGroup = currentGroup[currentGroup.Count - 1];
            if (record.limit == lastGroup.limit && record.on_date == lastGroup.end_date.AddDays(1)) {
                lastGroup.end_date = record.on_date;
            } else {
                currentGroup.Add(new GroupedData {
                    company_vat_number = vatNumber,
                    limit = record.limit,
                    start_date = record.on_date,
                    end_date = record.on_date
                });
            }
        }
        foreach (KeyValuePair<string, List<GroupedData>> entry in groups) {
            groupedData.AddRange(entry.Value);
        }
        return groupedData;
    }
}