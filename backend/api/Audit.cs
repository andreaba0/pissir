public class Audit
{

    private PostgresPool postgresPool;
    public Audit(
        string host,
        string port,
        string database,
        string user,
        string password
    )
    {
        this.postgresPool = new PostgresPool(
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
    }
}