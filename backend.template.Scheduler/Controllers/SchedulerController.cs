using backend.template.Scheduler.Jobs;
using Microsoft.AspNetCore.Mvc;

namespace backend.template.Scheduler.Controllers
{
    [ApiController]
    [Route("api/scheduler")]
    public class SchedulerController : ControllerBase
    {
        private readonly MigrationJob _migrationJob;
        private readonly UpdateDatabaseJob _updateDatabaseJob;
        private readonly ILogger<SchedulerController> _logger;

        public SchedulerController(
            MigrationJob migrationJob,
            UpdateDatabaseJob updateDatabaseJob,
            ILogger<SchedulerController> logger)
        {
            _migrationJob = migrationJob;
            _updateDatabaseJob = updateDatabaseJob;
            _logger = logger;
        }

        [HttpPost("update/migration")]
        public async Task<IActionResult> RunMigration(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Starting migration job");
                var result = await _migrationJob.Execute(cancellationToken);
                _logger.LogInformation("Migration completed successfully");

                return Ok(new
                {
                    Success = true,
                    Message = "Migration completed successfully",
                    Details = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during migration");
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Migration failed",
                    Error = ex.Message
                });
            }
        }

        [HttpPost("update/database")]
        public async Task<IActionResult> UpdateDatabase(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Starting database update job");
                var result = await _updateDatabaseJob.Execute(cancellationToken);

                if (result.Success)
                {
                    _logger.LogInformation("Database update completed successfully");
                    return Ok(result);
                }

                _logger.LogWarning("Database update completed with warnings: {message}", result.Message);
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during database update");
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Database update failed",
                    Error = ex.Message
                });
            }
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetStatus(CancellationToken cancellationToken)
        {
            try
            {
                var dbStatus = await _updateDatabaseJob.GetDatabaseStatus(cancellationToken);
                return Ok(dbStatus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving database status");
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Failed to retrieve database status",
                    Error = ex.Message
                });
            }
        }
    }
}
