using backend.template.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Trace;
using System.Diagnostics;

namespace backend.template.Scheduler.Jobs;

public class UpdateDatabaseJob
{
    public const string ActivitySourceName = "DatabaseUpdates";
    private static readonly ActivitySource activitySource = new(ActivitySourceName);
    private readonly CoreContext _dbContext;
    private readonly ILogger<UpdateDatabaseJob> _logger;

    public UpdateDatabaseJob(CoreContext dbContext, ILogger<UpdateDatabaseJob> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<UpdateResult> Execute(CancellationToken cancellationToken = default)
    {
        using var activity = activitySource.StartActivity("DatabaseUpdates Job", ActivityKind.Client);

        try
        {
            _logger.LogInformation("Starting database update check");

            // Use execution strategy for better resilience
            var strategy = _dbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                // Get pending migrations before updating the database
                var pendingMigrations = await _dbContext.Database.GetPendingMigrationsAsync(cancellationToken);
                var pendingList = pendingMigrations.ToList();

                if (!pendingList.Any())
                {
                    _logger.LogInformation("No pending migrations found, Database is up to date");
                    return new UpdateResult
                    {
                        Success = true,
                        Message = "Database is up to date",
                        AppliedMigrations = Array.Empty<string>(),
                    };
                }

                _logger.LogInformation("Found {count} pending migrations: {migrations}",
                    pendingList.Count, string.Join(", ", pendingList));

                // Execute migrations within a transaction
                await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    await _dbContext.Database.MigrateAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);

                    return new UpdateResult
                    {
                        Success = true,
                        Message = $"Successfully applied {pendingList.Count} migrations",
                        AppliedMigrations = pendingList.ToArray()
                    };
                }
                catch
                {
                    await transaction.RollbackAsync(cancellationToken);
                    throw;
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during database update");
            activity?.RecordException(ex);
            return new UpdateResult
            {
                Success = false,
                Message = $"Update failed: {ex.Message}",
                Error = ex
            };
        }
    }

    public async Task<DatabaseStatus> GetDatabaseStatus(CancellationToken cancellationToken = default)
    {
        var pending = await _dbContext.Database.GetPendingMigrationsAsync(cancellationToken);
        var applied = await _dbContext.Database.GetAppliedMigrationsAsync(cancellationToken);

        return new DatabaseStatus
        {
            CurrentVersion = applied.LastOrDefault() ?? "No migrations applied",
            PendingMigrations = pending.ToList(),
            AppliedMigrations = applied.ToList(),
            IsUpToDate = !pending.Any()
        };
    }

}

public class UpdateResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string[] AppliedMigrations { get; set; } = Array.Empty<string>();
    public Exception? Error { get; set; }
}

public class DatabaseStatus
{
    public string CurrentVersion { get; set; } = string.Empty;
    public List<string> PendingMigrations { get; set; } = new();
    public List<string> AppliedMigrations { get; set; } = new();
    public bool IsUpToDate { get; set; }
}
