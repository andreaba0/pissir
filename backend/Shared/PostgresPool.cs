using Npgsql;

public class PostgresPool
{
    private NpgsqlDataSource pooledDataSource;
    private PostgresNode[] transactionDataSource;
    private int transactionTimeoutSeconds;
    private bool sharedPool;
    private bool transactionPool;


    public PostgresPool(
        string? host,
        string? port,
        string? database,
        string? user,
        string? password,
        int poolSize = 20,
        int transactionTimeoutSeconds = 60*5,
        bool sharedPool = true,
        bool transactionPool = true
    )
    {
        this.sharedPool = sharedPool;
        this.transactionPool = transactionPool;
        this.transactionTimeoutSeconds = transactionTimeoutSeconds;
        int transactionPoolSize = poolSize * 60 / 100;
        int pooledPoolSize = poolSize - transactionPoolSize;
        transactionDataSource = new PostgresNode[transactionPoolSize];
        if (host == null || port == null || database == null || user == null || password == null)
        {
            throw new System.Exception("PostgresPool() requires arguments");
        }
        string conn = $"Host={host};Port={port};Database={database};Username={user};Password={password}";
        pooledDataSource = NpgsqlDataSource.Create($"{conn};Pooling=true;Maximum Pool Size={pooledPoolSize};Minimum Pool Size=1");
        for (int i = 0; i < transactionDataSource.Length; i++)
        {
            NpgsqlDataSource dataSource = NpgsqlDataSource.Create($"{conn};Pooling=false");
            transactionDataSource[i] = new PostgresNode
            {
                conn = dataSource.OpenConnection(),
                dataSource = dataSource,
                busy = false
            };
        }
    }

    public PostgresPool()
    {
        throw new System.Exception("PostgresPool() requires arguments");
    }

    private int getFreeTransaction()
    {
        for (int i = 0; i < transactionDataSource.Length; i++)
        {
            Console.WriteLine(transactionDataSource[i].busy);
            if(transactionDataSource[i].busy&&transactionDataSource[i].lastInteraction!=null&&DateTime.Now.Subtract((DateTime)transactionDataSource[i].lastInteraction).TotalSeconds>this.transactionTimeoutSeconds)
            {
                transactionDataSource[i].conn?.Close();
                transactionDataSource[i].conn = transactionDataSource[i].dataSource?.OpenConnection();
                transactionDataSource[i].busy = false;
                transactionDataSource[i].lastInteraction = null;
            }
            if (!transactionDataSource[i].busy)
            {
                transactionDataSource[i].busy = true;
                return i;
            }
        }
        return -1;
    }

    public async Task<DatabaseResponse> queryTransaction(string query, object? args, int transaction)
    {
        if (transaction < 0 || transaction >= transactionDataSource.Length)
        {
            return new DatabaseResponse
            {
                success = false,
                error = "Invalid transaction"
            };
        }
        if (!transactionDataSource[transaction].busy)
        {
            return new DatabaseResponse
            {
                success = false,
                error = "Not a transaction"
            };
        }
        transactionDataSource[transaction].lastInteraction = DateTime.Now;
        try
        {
            await using var cmd = new NpgsqlCommand(query, transactionDataSource[transaction].conn);
            await using var reader = await cmd.ExecuteReaderAsync();
            List<object> result = new List<object>();
            while (await reader.ReadAsync())
            {
                result.Add(reader.GetFieldValue<object>(0));
            }
            return new DatabaseResponse
            {
                success = true,
                data = result
            };
        }
        catch (Exception e)
        {
            return new DatabaseResponse
            {
                success = false,
                error = e.ToString()
            };
        }
    }

    public async Task<DatabaseResponse> queryPooled(string query, object? args)
    {
        if(!this.sharedPool) {
            throw new System.Exception("Shared pool disabled");
        }
        NpgsqlConnection conn = await pooledDataSource.OpenConnectionAsync();
        try
        {
            await using var cmd = new NpgsqlCommand(query, conn);
            await using var reader = await cmd.ExecuteReaderAsync();
            List<object> result = new List<object>();
            while (await reader.ReadAsync())
            {
                result.Add(reader.GetFieldValue<object>(0));
            }
            await conn.CloseAsync();
            return new DatabaseResponse
            {
                success = true,
                data = result
            };
        }
        catch (Exception e)
        {
            await conn.CloseAsync();
            return new DatabaseResponse
            {
                success = false,
                error = e.ToString()
            };
        }

    }

    public async Task<DatabaseResponse> beginTransaction()
    {
        int transactionIndex = getFreeTransaction();
        if (transactionIndex < 0) return new DatabaseResponse
        {
            success = false,
            error = "No free transactions"
        };
        Task<DatabaseResponse> query = this.queryTransaction("BEGIN TRANSACTION ISOLATION LEVEL SERIALIZABLE", null, transactionIndex);
        DatabaseResponse response = await query;
        if(!response.success) return new DatabaseResponse {
            success = false,
            error = response.error
        };
        transactionDataSource[transactionIndex].lastInteraction = DateTime.Now;
        transactionDataSource[transactionIndex].busy = true;
        return new DatabaseResponse
        {
            success = true,
            data = transactionIndex
        };
    }

    public async Task<DatabaseResponse> commitTransaction(int transaction)
    {
        if (transaction < 0 || transaction >= transactionDataSource.Length) return new DatabaseResponse
        {
            success = false,
            error = "Invalid transaction"
        };
        if (!transactionDataSource[transaction].busy) return new DatabaseResponse
        {
            success = false,
            error = "Not a transaction"
        };
        Task<DatabaseResponse> res = this.queryTransaction("COMMIT", null, transaction);
        DatabaseResponse response = await res;
        if(!response.success) return new DatabaseResponse {
            success = false,
            error = response.error
        };
        transactionDataSource[transaction].busy = false;
        transactionDataSource[transaction].lastInteraction = null;
        return new DatabaseResponse
        {
            success = true
        };
    }

    public async Task<DatabaseResponse> rollbackTransaction(int transaction)
    {
        if (transaction < 0 || transaction > transactionDataSource.Length) return new DatabaseResponse
        {
            success = false,
            error = "Invalid transaction"
        };
        if (!transactionDataSource[transaction].busy) return new DatabaseResponse
        {
            success = false,
            error = "Not a transaction"
        };
        Task<DatabaseResponse> res = this.queryTransaction("ROLLBACK", null, transaction);
        DatabaseResponse response = await res;
        if(!response.success) return new DatabaseResponse {
            success = false,
            error = response.error
        };
        transactionDataSource[transaction].busy = false;
        transactionDataSource[transaction].lastInteraction = null;
        return new DatabaseResponse
        {
            success = true
        };
    }
}

class PostgresNode
{
    public NpgsqlDataSource? dataSource;
    public NpgsqlConnection? conn;
    public bool busy = false;
    public DateTime? lastInteraction = null;
}