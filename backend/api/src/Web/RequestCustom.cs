using System.Collections.Specialized;

namespace Web;

public class RequestCustom: IRequestCustom {
    public NameValueCollection Headers { get; set; }
}