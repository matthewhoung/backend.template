using backend.template.Data.Contexts;
using backend.template.Data.Enums;
using Microsoft.EntityFrameworkCore;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

#region DbConnection Configuration
var dbConnstr = builder.Configuration.GetConnectionString("Postgresql") ?? 
                throw new ArgumentException("Application connection string not found");

var dataSourceBuilder = new NpgsqlDataSourceBuilder(dbConnstr);
dataSourceBuilder.EnableDynamicJson()
                 .MapEnum<MeetingAssetType>();

builder.AddNpgsqlDbContext<CoreContext>("Postgresql", configureDbContextOptions: config =>
{
    config.UseNpgsql(dataSourceBuilder.Build());
});
#endregion

builder.AddServiceDefaults();

var app = builder.Build();

app.MapDefaultEndpoints();

app.MapGet("/api/db/status", async (CoreContext dbContext) =>
{
    try
    {
        var canConnect = await dbContext.Database.CanConnectAsync();
        return Results.Ok(new
        {
            Status = canConnect ? "Connected" : "Disconnected",
            Timestamp = DateTime.UtcNow,
            Database = dbContext.Database.GetDbConnection().Database,
            Server = dbContext.Database.GetDbConnection().DataSource
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new
        {
            Status = "Error",
            Message = ex.Message,
            Timestamp = DateTime.UtcNow
        });
    }
});

app.Run();