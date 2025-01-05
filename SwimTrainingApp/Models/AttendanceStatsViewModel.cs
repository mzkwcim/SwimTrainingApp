namespace SwimTrainingApp.Models
{
    public class AttendanceStatsViewModel
    {
        public User Athlete { get; set; }
        public int TotalTrainings { get; set; }
        public int PresentCount { get; set; }
        public double Percentage { get; set; }
    }
}
