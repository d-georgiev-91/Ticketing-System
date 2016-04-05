namespace TicketingSystem.Server.Models.User
{
    public class ChangePasswordBindingModel
    {
        public string OldPassword { get; set; }

        public string NewPassword { get; set; }
    }
}