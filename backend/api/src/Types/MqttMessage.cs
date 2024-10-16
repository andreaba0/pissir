namespace Types;

public class MqttMessage {
    public string vat_number { get; private set; }
    public string obj_id { get; private set; }
    public string obj_type { get; private set; }
    public string data { get; private set; }
    public string log_epoch { get; private set; }
    public MqttMessage(string vat_number, string obj_id, string obj_type, string data, string log_epoch) {
        this.vat_number = vat_number;
        this.obj_id = obj_id;
        this.obj_type = obj_type;
        this.data = data;
        this.log_epoch = log_epoch;
    }
}