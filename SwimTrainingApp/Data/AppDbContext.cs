using Microsoft.EntityFrameworkCore;
using SwimTrainingApp.Models;

namespace SwimTrainingApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Training> Trainings { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<TrainingTask> TrainingTasks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Attendance>()
                .HasOne(a => a.Training)
                .WithMany(t => t.Attendances)
                .HasForeignKey(a => a.TrainingId);
        }
    }
}
