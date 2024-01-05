using System.Collections.Specialized;

namespace Web;
public interface IRequestCustom
{
    public NameValueCollection Headers { get; set; }
}