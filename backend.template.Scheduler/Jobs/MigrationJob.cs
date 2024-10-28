using backend.template.Data.Contexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Diagnostics;
using OpenTelemetry.Trace;

namespace backend.template.Scheduler.Jobs;

public class MigrationJob
{
    public const string ActivitySourceName = "Migrations";
    private static readonly ActivitySource activitySource = new(ActivitySourceName);
    private readonly CoreContext _dbContext;
    private readonly ILogger<MigrationJob> _logger;

    public MigrationJob(CoreContext dbContext, ILogger<MigrationJob> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }


    public async Task<MigrationResult> Execute(CancellationToken cancellationToken = default)
    {
        using var activity = activitySource.StartActivity("MigrationJob Job", ActivityKind.Client);

        try
        {
            _logger.LogInformation("Starting database migration process");

            // First ensure database exists
            var databaseExists = await EnsureDatabaseAsync(_dbContext, cancellationToken);
            _logger.LogInformation("Database existence verified. Database {status}",
                databaseExists ? "already existed" : "was created");

            // Check if migration history table exists
            var historyExists = await CheckMigrationHistoryTableExists(cancellationToken);

            if (!historyExists)
            {
                _logger.LogInformation("Creating migration history table");
                await CreateMigrationHistoryTable(cancellationToken);
            }

            _logger.LogInformation("Database migrations completed successfully");

            return new MigrationResult
            {
                Success = true,
                DatabaseCreated = !databaseExists,
                HistoryTableCreated = !historyExists,
                Message = "Migration completed successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during database migration");
            activity?.RecordException(ex);
            throw;
        }
    }

    private async Task<bool> EnsureDatabaseAsync(CoreContext dbContext, CancellationToken cancellationToken)
    {
        var dbCreator = dbContext.GetService<IRelationalDatabaseCreator>();
        var strategy = dbContext.Database.CreateExecutionStrategy();
        var existed = false;

        await strategy.ExecuteAsync(async () =>
        {
            existed = await dbCreator.ExistsAsync(cancellationToken);
            if (!existed)
            {
                await dbCreator.CreateAsync(cancellationToken);
            }
        });

        return existed;
    }

    private async Task<bool> CheckMigrationHistoryTableExists(CancellationToken cancellationToken)
    {
        var exists = await _dbContext.Database.GetService<IRelationalDatabaseCreator>()
            .HasTablesAsync(cancellationToken);
        return exists;
    }

    private async Task CreateMigrationHistoryTable(CancellationToken cancellationToken)
    {
        var strategy = _dbContext.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                await _dbContext.Database.MigrateAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        });
    }
}

public class MigrationResult
{
    public bool Success { get; set; }
    public bool DatabaseCreated { get; set; }
    public bool HistoryTableCreated { get; set; }
    public string Message { get; set; } = string.Empty;
}
