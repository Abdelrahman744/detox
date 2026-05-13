using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using detox.Models;

namespace detox.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
{
    public DbSet<FocusBlock> FocusBlocks { get; set; }
    public DbSet<Habit>      Habits      { get; set; }
    public DbSet<HabitLog>   HabitLogs   { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Habit>(e =>
        {
            e.HasMany(h => h.Logs)
             .WithOne(l => l.Habit)
             .HasForeignKey(l => l.HabitId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(h => h.UserId);
        });

        builder.Entity<HabitLog>(e =>
        {
            // One log per habit per day per user
            e.HasIndex(l => new { l.HabitId, l.Date }).IsUnique();
            e.HasIndex(l => l.UserId);
        });
    }
}
