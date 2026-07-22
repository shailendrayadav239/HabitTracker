using Microsoft.EntityFrameworkCore;
using HabitTracker.Entities;

namespace HabitTracker.ApplicationDBContext
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<Signup> Signups { get; set; }

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
        }
    }
}