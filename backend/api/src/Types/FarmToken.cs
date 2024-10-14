namespace Types;

public class FarmToken {
    public string vat_number { get; private set; }
    public long epoch { get; private set; }
    public string path { get; private set; }
    public FarmToken(string vat_number, long epoch, string path) {
        this.vat_number = vat_number;
        this.epoch = epoch;
        this.path = path;
    }
}