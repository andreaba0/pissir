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

app.MapGet("/api/test", async () => {
    await Task.Run(() => postgresPool.queryPooled("SELECT NOW()", null));
    return "Hello World";
});

app.MapPost("/water/buy", async () => {
    return "Hello World!";
});

app.MapPost("/api/water/sell", () => {
    int transactionId = postgresPool.beginTransaction();
    postgresPool.queryTransaction("INSERT INTO test(qty, id) values(10, 'prova')", null, transactionId);
    postgresPool.commitTransaction(transactionId);
    return "Hello World!";
});

app.MapPost("/user/create", () => {
    return "Hello World!";
});

app.MapPost("/user/delete", () => {
    return "Hello World!";
});

app.Run();
