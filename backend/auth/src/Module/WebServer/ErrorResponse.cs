namespace Module.WebServer;

public class ErrorResponse {
    public Dictionary<string, List<string>> missing_fields {get;}
    public Dictionary<string, List<string>> invalid_fields {get;}
    public string reason {get;private set;}
    public ErrorResponse(
    ) {
        missing_fields = new Dictionary<string, List<string>>();
        missing_fields.Add("heaader", new List<string>());
        missing_fields.Add("request_body", new List<string>());
        invalid_fields = new Dictionary<string, List<string>>();
        invalid_fields.Add("heaader", new List<string>());
        invalid_fields.Add("request_body", new List<string>());
        reason = "";
    }

    public void AddMissingHeader(string content) {
        missing_fields["header"].Add(content);
    }
    public void AddMissingBody(string content) {
        missing_fields["request_body"].Add(content);
    }
    public void AddInvalidHeader(string content) {
        invalid_fields["header"].Add(content);
    }
    public void AddInvalidBody(string content) {
        invalid_fields["request_body"].Add(content);
    }
    public void SetReason(string content) {
        reason = content;
    }
}