using Microsoft.EntityFrameworkCore;
using SwimTrainingApp.Models;

namespace SwimTrainingApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Training> Trainings { get; set; }
        public DbSet<TrainingTask> TrainingTasks { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Example of Fluent API configuration
            modelBuilder.Entity<TrainingTask>()
                .Property(t => t.TaskDescription)
                .HasMaxLength(500)
                .IsRequired();

            modelBuilder.Entity<TrainingTask>()
                .Property(t => t.Distance)
                .IsRequired()
                .HasDefaultValue(0);
        }
    }
}
