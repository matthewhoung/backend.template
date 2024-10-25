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

app.Run();