public class Accountant {

    private PostgresPool postgresPool;
    public Accountant(
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
        Console.WriteLine("Accounting...");
    }
}