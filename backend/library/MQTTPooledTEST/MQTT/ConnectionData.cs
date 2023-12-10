namespace MQTTConcurrent;

public class ConnectionDataTest {

    [Test]
    public void parseTopicTest() {
        Assert.AreEqual(ConnectionData.parseTopic("signup/user"), "signup/user");
        Assert.AreEqual(ConnectionData.parseTopic("$share/group/signup/user/"), "signup/user");
    }

}