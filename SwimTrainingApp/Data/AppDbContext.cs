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

            // Relacja 1 do wielu między Training a TrainingTask
            modelBuilder.Entity<TrainingTask>()
                .HasOne(t => t.Training) // Nawigacja z `TrainingTask` do `Training`
                .WithMany(tr => tr.Tasks)  // Nawigacja z `Training` do `TrainingTask`
                .HasForeignKey(t => t.TrainingId) // Klucz obcy
                .OnDelete(DeleteBehavior.Cascade);
        }



    }
}
