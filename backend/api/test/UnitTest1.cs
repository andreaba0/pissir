using Utility;
using Moq;
using Npgsql;
using MQTTnet;
using MQTTnet.Formatter;
using MQTTnet.Client;

using System.Threading.Tasks;
using System.Threading.Channels;

namespace test;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Test1()
    {
        Assert.Pass();
    }

    [Test]
    public void TestUserRole()
    {
        User user = new User();
        user.role = "WSP";
        Assert.AreEqual(UserParser.getRole(user), UserRole.WSP);
        user.role = "FAR";
        Assert.AreEqual(UserParser.getRole(user), UserRole.FAR);
        user.role = "UNKNOW";
        Assert.AreEqual(UserParser.getRole(user), UserRole.UNKNOW);
    }

    [Test]
    public void TestProcessMessage()
    {
        Accountant accountant = new Accountant();
        var channelSQL = Channel.CreateUnbounded<User>(
            new UnboundedChannelOptions
            {
                SingleReader = false,
                SingleWriter = true
            }
        );
        string message = "{\"name\":\"Andrea\",\"id\":\"01ARZ3NDEKTSV4RRFFQ69G5FAV\",\"role\":\"WSP\"}";
        MqttApplicationMessage mqttMessage = new MqttApplicationMessageBuilder()
                    .WithTopic("test")
                    .WithPayload(message)
                    .Build();
        Task handler = Task.Run(async () =>
        {
            await accountant.processMessage(new MqttApplicationMessageReceivedEventArgs(
                "e4b855432db84b5587947ac3dc3ae79f",
                mqttMessage,
                MqttPacketFactories.Publish.Create(
                    mqttMessage
            ),
            async Task (MqttApplicationMessageReceivedEventArgs e, CancellationToken ct) => {

            }
            ), channelSQL);
        });
        handler.Wait();
        ValueTask<User> userTask = channelSQL.Reader.ReadAsync();
        User user = userTask.Result;
        Assert.AreEqual(user.name, "Andrea");
    }
}