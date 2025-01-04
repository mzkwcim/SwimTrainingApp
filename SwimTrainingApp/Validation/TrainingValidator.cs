using FluentValidation;
using SwimTrainingApp.Models;

namespace SwimTrainingApp.Validation
{
    public class TrainingValidator : AbstractValidator<Training>
    {
        public TrainingValidator()
        {
            RuleFor(t => t.Date)
                .GreaterThanOrEqualTo(DateTime.Now)
                .WithMessage("Date must be today or in the future.");

            RuleFor(t => t.Tasks)
                .NotEmpty()
                .WithMessage("Training must have at least one task.")
                .Must(tasks => tasks.All(task => task.Distance > 0))
                .WithMessage("All tasks must have a positive distance.");
        }
    }
}
