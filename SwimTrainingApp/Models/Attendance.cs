namespace SwimTrainingApp.Models
{
    public class Attendance
    {
        public int Id { get; set; }
        public int TrainingId { get; set; }
        public int AthleteId { get; set; }
        public bool IsPresent { get; set; }

    }

}
