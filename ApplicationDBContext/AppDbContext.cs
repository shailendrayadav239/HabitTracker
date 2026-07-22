using Microsoft.EntityFrameworkCore;
using HabitTracker.Entities;

namespace HabitTracker.ApplicationDBContext
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<Signup> Signups { get; set; }
        public DbSet<Habit> Habits { get; set; }
        public DbSet<HabitCompletion> HabitCompletions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Signup>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.HasIndex(s => s.Email).IsUnique();
                entity.Property(s => s.Email).IsRequired();
                entity.Property(s => s.Password).IsRequired();
            });

            modelBuilder.Entity<Habit>(entity =>
            {
                entity.HasKey(h => h.Id);
                entity.Property(h => h.Name).IsRequired();
                entity.Property(h => h.Frequency).IsRequired();
                entity.HasIndex(h => h.UserId);
            });

            modelBuilder.Entity<HabitCompletion>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.HasOne(c => c.Habit)
                      .WithMany(h => h.Completions)
                      .HasForeignKey(c => c.HabitId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(c => new { c.HabitId, c.Date }).IsUnique();
            });
        }
    }
}