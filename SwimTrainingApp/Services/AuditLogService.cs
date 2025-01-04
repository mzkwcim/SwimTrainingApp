using SwimTrainingApp.Data;
using SwimTrainingApp.Models;

namespace SwimTrainingApp.Services
{
    public class AuditLogService
    {
        private readonly AppDbContext _dbContext;

        public AuditLogService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task LogAsync(string action, string username)
        {
            var log = new AuditLog
            {
                Action = action,
                Username = username,
                Timestamp = DateTime.UtcNow
            };

            _dbContext.AuditLogs.Add(log);
            await _dbContext.SaveChangesAsync();
        }
    }
}
