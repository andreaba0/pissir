using Microsoft.VisualBasic;
using Npgsql;

public class Postgres {
    private object connection;
    public Postgres() {

    }

    public async void connect(
        short poolSize, 
        string host, 
        string port, 
        string database, 
        string user, 
        string password
    ) {
        await using var dataSource = NpgsqlDataSource.Create(
                $"Host={host};Username={user};Password={password};Database={database}"
            );
    }
}