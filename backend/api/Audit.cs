public class Audit
{

    private PostgresPool postgresPool;
    private MqttClientPool mqttClientPool;
    public Audit(
        string host,
        string port,
        string database,
        string user,
        string password
    )
    {
        mqttClientPool = new MqttClientPool();
        postgresPool = new PostgresPool(
            host,
            port,
            database,
            user,
            password
        );
    }

    public async Task runServer()
    {
        Console.WriteLine("Auditing...");
        await mqttClientPool.connectToBroker(
            "127.0.0.1",
            "1883"
        );
    }
}