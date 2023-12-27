using System;
using System.Text.RegularExpressions;

namespace MQTTConcurrent;

public class ConnectionData
{
    public string host { get; set; }
    public int port { get; set; }
    public string username { get; set; }
    public string password { get; set; }
    public int poolSize { get; set; }

    public ConnectionData() {
        this.host = "localhost";
        this.port = 1883;
        this.username = string.Empty;
        this.password = string.Empty;
        this.poolSize = 4;
    }

    public static ConnectionData Parse(string connectionString)
    {
        ConnectionData cData = new ConnectionData();
        Regex regexHost = new Regex(@"host=(?<host>[A-Za-z0-9-_\.\:\/]+)");
        Regex regexPort = new Regex(@"port=(?<port>[0-9]{4,5})");
        Regex regexUsername = new Regex(@"username=(?<username>[A-Za-z0-9_]+)");
        Regex regexPassword = new Regex(@"password=(?<password>[A-Za-z0-9_]+)");
        Regex regexPoolSize = new Regex(@"poolSize=(?<poolSize>[0-9]{1,3})");
        Match matchHost = regexHost.Match(connectionString);
        if (matchHost.Success&&matchHost.Groups["host"].Value!=null)
        {
            cData.host = matchHost.Groups["host"].Value;
        }
        Match matchPort = regexPort.Match(connectionString);
        if (matchPort.Success&&matchPort.Groups["port"].Value!=null)
        {
            string port = matchPort.Groups["port"].Value;
            bool isInt = int.TryParse(port, out int newPort);
            if(isInt)
                cData.port = newPort;
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
        if (matchPoolSize.Success&&matchPoolSize.Groups["poolSize"].Value!=null)
        {
            bool isInt = int.TryParse(matchPoolSize.Groups["poolSize"].Value, out int newSize);
            if(isInt)
                cData.poolSize = newSize;
        }
        return cData;
    }

    public static string parseTopic(string topic) {
        Regex regexTopic = new Regex(@"^(?<lb>(\$share\/[A-Za-z0-9\-_]+\/)?)(?<path>([A-Za-z0-9\-_]+\/)*[A-Za-z0-9\-_]+)$");
        Match matchTopic = regexTopic.Match(topic);
        //check for path only
        if (matchTopic.Success) {
            return matchTopic.Groups["path"].Value;
        }
        return string.Empty;
    }
}