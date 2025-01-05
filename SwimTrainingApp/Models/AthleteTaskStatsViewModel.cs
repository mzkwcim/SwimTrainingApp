namespace SwimTrainingApp.Models
{
    public class AthleteTaskStatsViewModel
    {
        public int AthleteId { get; set; }
        public string AthleteName { get; set; }
        public int TotalDistance { get; set; }
        public List<TaskTypeStats> TaskDistribution { get; set; }
    }
}
