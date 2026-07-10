using Microsoft.EntityFrameworkCore;

namespace HabitTrackerProject.ApplicationDBContext
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        // DbSets added as features are built:
        // public DbSet<Signup> Signups { get; set; }
        // public DbSet<Habit> Habits { get; set; }
        // public DbSet<HabitCompletion> HabitCompletions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}