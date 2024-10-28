using backend.template.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.template.Data.Contexts;

public class CoreContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Meeting> Meetings => Set<Meeting>();
    public DbSet<MeetingAsset> MeetingAssets => Set<MeetingAsset>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
    }
}
