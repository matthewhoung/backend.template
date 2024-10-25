using backend.template.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(config =>
{
    config.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Backend Template API",
        Version = "v1",
        Description = "API endpoints for backend template"
    });
});

builder.AddNpgsqlDbContext<CoreContext>("backend_template");
builder.AddServiceDefaults();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Backend Template API V1");
        c.RoutePrefix = "swagger";
    });
}

app.MapDefaultEndpoints();

app.MapGet("/api/database/connection-test", async (CoreContext db) =>
{
    try
    {
        // Test database connection
        var canConnect = await db.Database.CanConnectAsync();

        // Get connection string (be careful with exposing this in production)
        var connectionString = db.Database.GetConnectionString();

        return Results.Ok(new
        {
            IsConnected = canConnect,
            ConnectionString = connectionString,
            DatabaseProvider = db.Database.ProviderName,
            ServerVersion = db.Database.GetDbConnection().ServerVersion
        });
    }
    catch (Exception ex)
    {
        return Results.Problem(
            title: "Database Connection Error",
            detail: ex.Message,
            statusCode: 500
        );
    }
})
.WithName("TestDatabaseConnection")
.WithOpenApi(operation => new OpenApiOperation(operation)
{
    Summary = "Test database connection and get connection info",
    Description = "Returns the connection status and basic information about the database connection"
})
.WithTags("Database");

app.Run();
