using KeyloopScheduler.Api.Data;
using Microsoft.EntityFrameworkCore;



var builder = WebApplication.CreateBuilder(args);

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' was not found.");
Console.WriteLine(
    $"DefaultConnection configured: {!string.IsNullOrWhiteSpace(connectionString)}");
Console.WriteLine(
    $"Connection target: {new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connectionString).DataSource}");
Console.WriteLine(
    $"Database name: {new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connectionString).InitialCatalog}");
builder.Services.AddDbContext<SchedulerDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactFrontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseCors("AllowReactFrontend");
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider
        .GetRequiredService<SchedulerDbContext>();

    await DbInitializer.SeedAsync(db);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.MapGet("/", () => Results.Ok(new
{
    application = "Keyloop Service Scheduler",
    status = "Running"
}));

app.MapGet("/api/health/database", async (
    SchedulerDbContext db,
    CancellationToken cancellationToken) =>
{
    try
    {
        var canConnect = await db.Database.CanConnectAsync(
            cancellationToken);

        return canConnect
            ? Results.Ok(new { status = "Connected" })
            : Results.Problem(
                title: "Database unavailable",
                statusCode: StatusCodes.Status503ServiceUnavailable);
    }
    catch (Exception ex)
    {
        app.Logger.LogError(
            ex,
            "Database connectivity check failed");

        return Results.Problem(
            title: "Database connectivity check failed",
            detail: ex.Message,
            statusCode: StatusCodes.Status503ServiceUnavailable);
    }
});

app.Run();
