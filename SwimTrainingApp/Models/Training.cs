using System.ComponentModel.DataAnnotations;


namespace SwimTrainingApp.Models
{
    public class Training
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public ICollection<Attendance> Attendances { get; set; } // Navigation property
        public List<TrainingTask> Tasks { get; set; } = new List<TrainingTask>();
    }

}
