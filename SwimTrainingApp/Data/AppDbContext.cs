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
        public DbSet<AuditLog> AuditLogs { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Konfiguracja relacji między Training a TrainingTask
            modelBuilder.Entity<TrainingTask>()
                .HasOne(t => t.Training)
                .WithMany(t => t.Tasks)
                .HasForeignKey(t => t.TrainingId)
                .OnDelete(DeleteBehavior.Cascade); // Zadania są usuwane razem z treningiem
        }

    }
}
