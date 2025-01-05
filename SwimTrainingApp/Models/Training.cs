using System.ComponentModel.DataAnnotations;


namespace SwimTrainingApp.Models
{
    public class Training
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Date is required.")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        public List<TrainingTask> Tasks { get; set; } = new List<TrainingTask>();
    }

}
