using backend.template.Data.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Npgsql;

namespace backend.template.Data.Contexts;

public class CoreContextFactory : IDesignTimeDbContextFactory<CoreContext>
{
    public CoreContext CreateDbContext(string[] args)
    {
        const string connectionString = "Host=localhost;Port=5432;Database=template;Username=postgres;Password=postgres";

        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.EnableDynamicJson()
            .MapEnum<MeetingAssetType>();

        var options = new DbContextOptionsBuilder<CoreContext>()
            .UseNpgsql(dataSourceBuilder.Build())
            .Options;

        return new CoreContext(options);
    }
}
