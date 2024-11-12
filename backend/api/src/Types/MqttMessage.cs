namespace Types;

public class MqttMessageIot {
    public string vat_number { get; private set; }
    public string field_id { get; private set; }
    public string obj_id { get; private set; }
    public string type { get; private set; }
    public string data { get; private set; }
    public long log_timestamp { get; private set; }
    public MqttMessageIot(string vat_number, string field_id, string obj_id, string type, string data, long log_timestamp) {
        this.vat_number = vat_number;
        this.field_id = field_id;
        this.obj_id = obj_id;
        this.type = type;
        this.data = data;
        this.log_timestamp = log_timestamp;
    }
}