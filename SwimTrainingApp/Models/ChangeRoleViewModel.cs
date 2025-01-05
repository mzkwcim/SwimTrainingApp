namespace SwimTrainingApp.Models
{
    public class ChangeRoleViewModel
    {
        public List<User> Users { get; set; } = new List<User>();
        public int SelectedUserId { get; set; }
        public UserRole NewRole { get; set; }
        public List<UserRole> AvailableRoles { get; set; } = new List<UserRole>();
    }
}
