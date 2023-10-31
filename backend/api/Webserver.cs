public class Webserver
{
    private PostgresPool postgresPool;
    public Webserver(
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

    public void runServer()
    {
        var builder = WebApplication.CreateBuilder();
        var configuration = builder.Configuration;
        var app = builder.Build();
        app.Use(async (context, next) =>
        {
            //get jwt bearer from header and check if it's valid
            string? jwtBearer = context.Request.Headers["Authorization"];
            if (jwtBearer == null)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Missing Authorization header");
                return;
            }
            if (!jwtBearer.StartsWith("Bearer "))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Invalid Authorization header");
                return;
            }
            string jwt = jwtBearer.Substring(7);
            TokenOut instance = JwtTokenManager.jwtVerified(jwt);
            if (instance.success == false)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync(instance.error);
                return;
            }
            if (!instance.claims.TryGetValue("role", out _))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Missing role claim");
                return;
            }
            if (!instance.claims.TryGetValue("user_id", out _))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Missing user_id claim");
                return;
            }
            postgresPool.queryPooled("SELECT 1 FROM users WHERE id=$1", new object[] { instance.claims["user_id"] });
            //if everything is ok, continue
            await next();
        });

        app.MapPost("/api/water/sell", async context =>
        {
            Task<DatabaseResponse> taskTransaction = postgresPool.beginTransaction();
            DatabaseResponse transaction = await taskTransaction;
            if (!transaction.success)
            {
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync(transaction.error ?? "Generic error");
                return;
            }
            int transactionId = (int)(transaction.data ?? -1);
            Task<DatabaseResponse> insertQuery = postgresPool.queryTransaction("INSERT INTO test(qty, id) values(10, 'prova')", null, transactionId);
            DatabaseResponse insert = await insertQuery;
            if (!insert.success)
            {
                await postgresPool.rollbackTransaction(transactionId);
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync(insert.error ?? "Generic error");
                return;
            }
            Task<DatabaseResponse> commitQuery = postgresPool.commitTransaction(transactionId);
            DatabaseResponse commit = await commitQuery;
            if (!commit.success)
            {
                await postgresPool.rollbackTransaction(transactionId);
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync(commit.error ?? "Generic error");
                return;
            }
            context.Response.StatusCode = 200;
            await context.Response.WriteAsync("Hello World!");
        });

        app.MapPost("/api/water/buy", async context =>
        {
            context.Response.StatusCode = 200;
            await context.Response.WriteAsync("Hello World!");
        });
        app.MapPost("/api/water/offer", async context =>
        {
            context.Response.StatusCode = 200;
            await context.Response.WriteAsync("Hello World!");
        });
        app.MapPost("/api/company/field", async context =>
        {
            context.Response.StatusCode = 200;
            await context.Response.WriteAsync("Hello World!");
        });
        app.MapDelete("/api/company/field", async context =>
        {
            context.Response.StatusCode = 200;
            await context.Response.WriteAsync("Hello World!");
        });
        app.MapGet("/api/analytics/field", async context =>
        {
            context.Response.StatusCode = 200;
            await context.Response.WriteAsync("Hello World!");
        });

        app.Run();
    }
}