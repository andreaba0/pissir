using Npgsql;

public class PostgresPool
{
    private NpgsqlDataSource pooledDataSource;
    private PostgresNode[] transactionDataSource = new PostgresNode[10];
    public PostgresPool(
        string? host,
        string? port,
        string? database,
        string? user,
        string? password
    )
    {
        if (host == null || port == null || database == null || user == null || password == null)
        {
            throw new System.Exception("PostgresPool() requires arguments");
        }
        string conn = $"Host={host};Port={port};Database={database};Username={user};Password={password}";
        pooledDataSource = NpgsqlDataSource.Create($"{conn};Pooling=true;Maximum Pool Size=10;Minimum Pool Size=5");
        for (int i = 0; i < transactionDataSource.Length; i++)
        {
            transactionDataSource[i] = new PostgresNode
            {
                conn = NpgsqlDataSource.Create($"{conn};Pooling=false").OpenConnection(),
                busy = false
            };
        }
    }

    public PostgresPool()
    {
        throw new System.Exception("PostgresPool() requires arguments");
    }

    private async void runQuery(string query, object args, int? transaction = null)
    {
        NpgsqlConnection conn;
        if (transaction == null)
        {
            conn = await pooledDataSource.OpenConnectionAsync();
        }
        else
        {
            int transactionIndex = (int)transaction;
            conn = transactionDataSource[transactionIndex].conn;
        }
        await using var cmd = new NpgsqlCommand(query, conn);
        /*{
            Parameters = {
                new NpgsqlParameter("args", args)
            }
        };*/
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var now = reader.GetFieldValue<DateTime>(0);
            System.Console.WriteLine(now);
        }
        if (transaction == null)
        {
            await conn.CloseAsync();
        }
    }

    private int getFreeTransaction()
    {
        for (int i = 0; i < transactionDataSource.Length; i++)
        {
            if (!transactionDataSource[i].busy)
            {
                transactionDataSource[i].busy = true;
                return i;
            }
        }
        return -1;
    }

    public void queryTransaction(string query, object args, int transaction)
    {
        if (transaction < 0 || transaction >= transactionDataSource.Length)
        {
            throw new System.Exception("Invalid transaction");
        }
        runQuery(query, args, transaction);
    }

    public async void queryPooled(string query, object args)
    {
        await Task.Run(() => runQuery(query, args));
    }

    public int beginTransaction()
    {
        int transactionIndex = getFreeTransaction();
        if (transactionIndex <0) return -1;
        runQuery("BEGIN TRANSACTION ISOLATION LEVEL SERIALIZABLE", null, transactionIndex);
        transactionDataSource[transactionIndex].lastInteraction = DateTime.Now;
        transactionDataSource[transactionIndex].busy = true;
        return transactionIndex;
    }

    public void commitTransaction(int transaction)
    {
        if(transaction < 0 || transaction >= transactionDataSource.Length)
        {
            throw new System.Exception("Invalid transaction");
        }
        runQuery("COMMIT", null, transaction);
        transactionDataSource[transaction].busy = false;
        transactionDataSource[transaction].lastInteraction = null;
    }

    public void rollbackTransaction(int transaction)
    {
        if(transaction<0||transaction>transactionDataSource.Length) {
            throw new System.Exception("Invalid transaction");
        }
        runQuery("ROLLBACK", null, transaction);
        transactionDataSource[transaction].busy = false;
        transactionDataSource[transaction].lastInteraction = null;
    }
}

class PostgresNode
{
    public NpgsqlConnection conn;
    public bool busy;
    public DateTime? lastInteraction = null;
}