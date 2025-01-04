namespace SwimTrainingApp.Models
{
    public class Training
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public ICollection<TrainingTask> Tasks { get; set; } = new List<TrainingTask>();
    }
}
