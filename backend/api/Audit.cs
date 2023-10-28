public class Audit {

    private PostgresPool postgresPool;
    public Audit(
        string host,
        string port,
        string database,
        string user,
        string password
    )
    {
        postgresPool = new PostgresPool(
            host,
            port,
            database,
            user,
            password
        );
    }

    public void runServer() {
        Console.WriteLine("Auditing...");
    }
}