using System.Threading.Channels;

namespace MQTTConcurrent;

public class RoundRobinDispatcherTest
{
    /*[Test]
    public async Task PushAllTest()
    {
        RoundRobinDispatcher dispatcher = new RoundRobinDispatcher(3);
        int counter = 0;
        await dispatcher.PushAll(new Message.MqttChannelSubscribe(
            "/signup/user"
        ));
        for (int i = 0; i < 3; i++)
        {
            Channel<IMqttChannelBus> channel = dispatcher.GetChannel(i);
            IMqttChannelBus message = await channel.Reader.ReadAsync();
            //Assert.AreEqual("topic", message.Topic);
            Assert.AreEqual(MqttChannelMessageType.SUBSCRIBE, message.Type);
            Assert.IsInstanceOf<Message.MqttChannelSubscribe>(message);
            counter++;
        }
        Assert.AreEqual(3, counter);
    }*/
}