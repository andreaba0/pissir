var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var app = builder.Build();

PostgresPool postgresPool = new PostgresPool(
    configuration["database:api:host"],
    configuration["database:api:port"],
    configuration["database:api:database"],
    configuration["database:api:user"],
    configuration["database:api:password"]
);

app.Use(async (context, next) =>
{
await next();
});

app.MapGet("/api/test", () =>
{
    postgresPool.queryPooled("SELECT NOW()", null);
    return "Hello World";
});

app.MapPost("/water/buy", () =>
{
    return "Hello World!";
});

app.MapPost("/api/water/sell", async context =>
{
    Task<DatabaseResponse> taskTransaction = postgresPool.beginTransaction();
    DatabaseResponse transaction = await taskTransaction;
    if(!transaction.success)
    {
        context.Response.StatusCode = 500;
        await context.Response.WriteAsync(transaction.error ?? "Generic error");
        return;
    }
    int transactionId = (int)(transaction.data ?? -1);
    Task<DatabaseResponse> insertQuery = postgresPool.queryTransaction("INSERT INTO test(qty, id) values(10, 'prova')", null, transactionId);
    DatabaseResponse insert = await insertQuery;
    if(!insert.success)
    {
        await postgresPool.rollbackTransaction(transactionId);
        context.Response.StatusCode = 500;
        await context.Response.WriteAsync(insert.error ?? "Generic error");
        return;
    }
    Task<DatabaseResponse> commitQuery = postgresPool.commitTransaction(transactionId);
    DatabaseResponse commit = await commitQuery;
    if(!commit.success)
    {
        await postgresPool.rollbackTransaction(transactionId);
        context.Response.StatusCode = 500;
        await context.Response.WriteAsync(commit.error ?? "Generic error");
        return;
    }
    context.Response.StatusCode = 200;
    await context.Response.WriteAsync("Hello World!");
});

app.MapPost("/user/create", () =>
{
    return "Hello World!";
});

app.MapPost("/user/delete", () =>
{
    return "Hello World!";
});

app.Run();
