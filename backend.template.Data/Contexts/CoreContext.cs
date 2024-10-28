using backend.template.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.template.Data.Contexts;

public class CoreContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Meeting> Meetings => Set<Meeting>();
    public DbSet<MeetingAsset> MeetingAssets => Set<MeetingAsset>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //base.OnModelCreating(modelBuilder);
        /*
        #region Configure Meeting entity
        modelBuilder.Entity<Meeting>(entity =>
        {
            entity.ToTable("meetings");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(50);
        });
        #endregion

        #region Configure MeetingAsset entity
        modelBuilder.Entity<MeetingAsset>(entity =>
        {
            entity.ToTable("meeting_assets");
            entity.HasKey(e => e.Seq);
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(50);
            entity.Property(e => e.Path)
                .IsRequired()
                .HasMaxLength(255);
            entity.Property(e => e.AssetType)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(25);

            // Add foreign key relationship
            entity.HasOne<Meeting>()
                .WithMany()
                .HasForeignKey(e => e.MeetingId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        #endregion
        */
    }
}
