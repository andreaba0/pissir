namespace MQTTConcurrent;

public class ConnectionData
{
    public string host { get; set; }
    public int port { get; set; }
    public string username { get; set; }
    public string password { get; set; }
    public int poolSize { get; set; }

    public static ConnectionData Parse(string connectionString)
    {
        ConnectionData cData = new ConnectionData();
        Regex regexHost = new Regex(@"host=(?<host>[A-Za-z0-9-_\.\:\/]+)");
        Regex regexPort = new Regex(@"port=(?<port>[0-9]{4,5})");
        Regex regexUsername = new Regex(@"username=(?<username>[A-Za-z0-9_]+)");
        Regex regexPassword = new Regex(@"password=(?<password>[A-Za-z0-9_]+)");
        Regex regexPoolSize = new Regex(@"poolSize=(?<poolSize>[0-9]{1,3})");
        Match matchHost = regexHost.Match(connectionString);
        if (matchHost.Success)
        {
            cData.host = matchHost.Groups["host"].Value ?? string.Empty;
        }
        Match matchPort = regexPort.Match(connectionString);
        if (matchPort.Success)
        {
            cData.port = int.Parse(matchPort.Groups["port"].Value) ?? 1883;
        }
        Match matchUsername = regexUsername.Match(connectionString);
        if (matchUsername.Success)
        {
            cData.username = matchUsername.Groups["username"].Value ?? string.Empty;
        }
        Match matchPassword = regexPassword.Match(connectionString);
        if (matchPassword.Success)
        {
            cData.password = matchPassword.Groups["password"].Value ?? string.Empty;
        }
        Match matchPoolSize = regexPoolSize.Match(connectionString);
        if (matchPoolSize.Success)
        {
            cData.poolSize = int.Parse(matchPoolSize.Groups["poolSize"].Value) ?? -1;
        }
        return cData;
    }
}