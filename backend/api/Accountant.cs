using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using MQTTnet.Packets;
using MQTTnet.Formatter;
using MQTTnet.Exceptions;

using System.Text.Json;

public class Accountant
{

    private string mqtt_host;
    private string mqtt_port;
    public Accountant(
        string mqtt_host,
        string mqtt_port
    )
    {
        this.mqtt_host = mqtt_host;
        this.mqtt_port = mqtt_port;
    }

    public async Task runServer()
    {

        while (true) {
            try {
                await ConnectToBroker();
                break;
            } catch (MqttCommunicationException e) {
                Console.WriteLine("Connection failed");
            } catch (Exception e) {
                Console.WriteLine(e);
                Console.WriteLine("+++++++++++++++++++++++++");
            } finally {
                System.Threading.Thread.Sleep(5000);
            }
        }
    }

    public async Task ConnectToBroker()
    {
        var mqttFactory = new MqttFactory();
        var mqttClient = mqttFactory.CreateMqttClient();
        var mqttClientOptions = new MqttClientOptionsBuilder().WithTcpServer($"{this.mqtt_host}").WithProtocolVersion(MqttProtocolVersion.V500).Build();

        mqttClient.ApplicationMessageReceivedAsync += processMessage;

        await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

        var mqttSubscribeOptions = new MqttClientSubscribeOptionsBuilder()
            .WithTopicFilter("$share/api/user/signup")
            .Build();

        await mqttClient.SubscribeAsync(mqttSubscribeOptions);

        while (true) {
            //check if connection is still alive
            if (!mqttClient.IsConnected) {
                //close connection
                await mqttClient.DisconnectAsync();
                throw new MqttCommunicationException("Connection lost");
            }

            System.Threading.Thread.Sleep(5000);
        }
    }

    private async Task processMessage(MqttApplicationMessageReceivedEventArgs e)
    {
        Console.WriteLine("### RECEIVED APPLICATION MESSAGE ###");
        Console.WriteLine(e.ApplicationMessage.Topic);

        //parse payload
        var payload = System.Text.Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
        var payloadJson = JsonSerializer.Deserialize<User>(payload);

        UserParser userParser = new UserParser(payloadJson);

        Console.WriteLine($"User role: {userParser.getRole()}");
        Console.WriteLine($"Valid id: {userParser.isValidId()}");

        return;
    }
}

class User {
    public string id { get; set; }
    public string codice_fiscale { get; set; }
    public string name { get; set; }
    public string surname { get; set; }
    public string company { get; set; }
    public string role { get; set; }
}

class UserParser : User {
    private User user;
    public UserParser(User user) {
        this.user = user;
    }

    public Role getRole() {
        if (this.user?.role == "GSI") {
            return Role.GSI;
        } else if (this.user?.role == "UA") {
            return Role.UA;
        } else {
            return Role.UNKNOW;
        }
    }

    public bool isValidId() {
        if(Ulid.TryParse(this.user?.id, out var id)) {
            return true;
        } else {
            return false;
        }
    }
}

enum Role {
    GSI,
    UA,
    UNKNOW
}