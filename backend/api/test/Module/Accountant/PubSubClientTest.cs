using Moq;

using MQTTnet;
using MQTTnet.Client;

namespace Module.Accountant {
    public class PubSubClientTest {
        [Test]
        public async Task TestConnectOnce() {
            var mqttClient = new Mock<IMqttClient>();
            mqttClient.SetupGet(x => x.IsConnected).Returns(false);
            mqttClient.Setup(x => x.ConnectAsync(It.IsAny<MqttClientOptions>(), It.IsAny<CancellationToken>()  )).Throws(new Exception());
            PubSubClient pubSubClient = new PubSubClient(mqttClient.Object);

            mqttClient.SetupGet(x => x.IsConnected).Returns(false);
            Assert.IsFalse(await pubSubClient.ConnectOnce());

            mqttClient.SetupGet(x => x.IsConnected).Returns(true);
            Assert.IsTrue(await pubSubClient.ConnectOnce());
        }
    }
}