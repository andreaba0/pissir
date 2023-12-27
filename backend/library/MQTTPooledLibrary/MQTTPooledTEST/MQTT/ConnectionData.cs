namespace MQTTConcurrent;

public class ConnectionDataTest {

    [Test]
    public void parseTopicTest() {
        Assert.That(ConnectionData.parseTopic("signup/user"), Is.EqualTo("signup/user"));
        Assert.That(ConnectionData.parseTopic("$share/group/signup/user"), Is.EqualTo("signup/user"));
        Assert.That(ConnectionData.parseTopic("signup"), Is.EqualTo("signup"));
        Assert.That(ConnectionData.parseTopic("$share/signup"), Is.EqualTo(string.Empty));
        Assert.That(ConnectionData.parseTopic("signup/user/"), Is.EqualTo(string.Empty));
        Assert.That(ConnectionData.parseTopic("signup/////user"), Is.EqualTo(string.Empty));
    }

    [Test]
    public void parseTest() {
        ConnectionData cData = ConnectionData.Parse("host=example.com;port=1883;username=;password=;poolSize=8");
        Assert.That(cData.host, Is.EqualTo("example.com"));
        Assert.That(cData.port, Is.EqualTo(1883));
        Assert.That(cData.username, Is.EqualTo(string.Empty));
        Assert.That(cData.password, Is.EqualTo(string.Empty));
        Assert.That(cData.poolSize, Is.EqualTo(8));


        cData = ConnectionData.Parse("host=localhost;port=1883;username=;password=;poolSize=8;");
        Assert.That(cData.host, Is.EqualTo("localhost"));
        Assert.That(cData.port, Is.EqualTo(1883));
        Assert.That(cData.username, Is.EqualTo(string.Empty));
        Assert.That(cData.password, Is.EqualTo(string.Empty));
        Assert.That(cData.poolSize, Is.EqualTo(8));


        cData = ConnectionData.Parse("host=localhost;port=1883;username=;password=;poolSize=4;extra=extra");
        Assert.That(cData.host, Is.EqualTo("localhost"));
        Assert.That(cData.port, Is.EqualTo(1883));
        Assert.That(cData.username, Is.EqualTo(string.Empty));
        Assert.That(cData.password, Is.EqualTo(string.Empty));
        Assert.That(cData.poolSize, Is.EqualTo(4));


        cData = ConnectionData.Parse("host=localhost;port=10005;username=;password=;poolSize=4;extra=extra;");
        Assert.That(cData.host, Is.EqualTo("localhost"));
        Assert.That(cData.port, Is.EqualTo(10005));
        Assert.That(cData.username, Is.EqualTo(string.Empty));
        Assert.That(cData.password, Is.EqualTo(string.Empty));
        Assert.That(cData.poolSize, Is.EqualTo(4));


        cData = ConnectionData.Parse("host=localhost;port=1883;username=;password=;poolSize=4;extra=extra;extra2=extra2");
        Assert.That(cData.host, Is.EqualTo("localhost"));
        Assert.That(cData.port, Is.EqualTo(1883));
        Assert.That(cData.username, Is.EqualTo(string.Empty));
        Assert.That(cData.password, Is.EqualTo(string.Empty));
        Assert.That(cData.poolSize, Is.EqualTo(4));


        cData = ConnectionData.Parse("host=localhost;port=1883;username=user;password=password;extra=extra;extra2=extra2;");
        Assert.That(cData.host, Is.EqualTo("localhost"));
        Assert.That(cData.port, Is.EqualTo(1883));
        Assert.That(cData.username, Is.EqualTo("user"));
        Assert.That(cData.password, Is.EqualTo("password"));
        Assert.That(cData.poolSize, Is.EqualTo(4));
    }

}