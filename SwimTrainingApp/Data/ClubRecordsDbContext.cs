using Microsoft.EntityFrameworkCore;
using SwimTrainingApp.Models;

namespace SwimTrainingApp.Data
{
    public class ClubRecordsDbContext : DbContext
    {
        public ClubRecordsDbContext(DbContextOptions<ClubRecordsDbContext> options)
            : base(options)
        {
        }

        public DbSet<ClubRecord> ClubRecords { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ClubRecord>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Distance).IsRequired().HasMaxLength(255);
                entity.Property(e => e.AthleteName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.ReadableTime).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Date).IsRequired().HasMaxLength(50);
                entity.Property(e => e.City).HasMaxLength(255);
                entity.Property(e => e.Time).IsRequired();
            });
        }
    }

    public class ClubRecord
    {
        public int Id { get; set; }
        public string Distance { get; set; }
        public string AthleteName { get; set; }
        public string ReadableTime { get; set; }
        public string Date { get; set; }
        public string City { get; set; }
        public double Time { get; set; }
    }
}
