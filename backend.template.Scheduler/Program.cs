using backend.template.Data.Contexts;
using backend.template.Scheduler.Jobs;
using Microsoft.EntityFrameworkCore;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

#region DbConnection Configuration
var dbConnstr = builder.Configuration.GetConnectionString("Postgresql") ??
                throw new ArgumentException("Application connection string not found");

var dataSourceBuilder = new NpgsqlDataSourceBuilder(dbConnstr);
dataSourceBuilder.EnableDynamicJson();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddLogging(logging =>
    {
        logging.AddConsole();
        logging.AddDebug();
    });
}

builder.AddNpgsqlDbContext<CoreContext>("Postgresql", configureDbContextOptions: config =>
{
    config.UseNpgsql(dataSourceBuilder.Build(), npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorCodesToAdd: null);
    });
});
#endregion

builder.Services.AddTransient<MigrationJob>();
builder.Services.AddTransient<UpdateDatabaseJob>();
builder.Services.AddControllers();

var app = builder.Build();


app.MapDefaultEndpoints();
app.Run();
