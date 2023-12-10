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
    public struct defaultValue {
        public const string host = "localhost";
        public const int port = 1883;
        public const int poolSize = 4;
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
        if (matchHost.Success)
        {
            cData.host = matchHost.Groups["host"].Value ?? defaultValue.host;
        }
        Match matchPort = regexPort.Match(connectionString);
        if (matchPort.Success)
        {
            if(!int.TryParse(matchPort.Groups["port"].Value, out port))
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
            if(!int.TryParse(matchPoolSize.Groups["poolSize"].Value, out cData.poolSize))
                cData.poolSize = defaultValue.poolSize;
        }
        return cData;
    }

    public static string parseTopic(string topic) {
        Regex regexTopic = new Regex(@"^(?<lb>\$share\/[A-Za-z0-9\-_]+)?\/?(?<path>[A-Za-z0-9\/\-_]+)$");
        Match matchTopic = regexTopic.Match(topic);
        //check for path only
        if (matchTopic.Success) {
            return matchTopic.Groups["path"].Value;
        }
        return string.Empty;
    }
}