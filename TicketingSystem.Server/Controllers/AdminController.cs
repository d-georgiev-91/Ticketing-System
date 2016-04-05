namespace TicketingSystem.Server.Controllers
{
    using Microsoft.AspNet.Identity.EntityFramework;

    using TicketingSystem.Data;
    using TicketingSystem.Models;

    public class AdminController: BaseApiController
    {
        private ApplicationUserManager userManager;

        public AdminController()
            : this(new TicketingSystemData())
        {
            
        }

        public AdminController(ITicketingSystemData data)
            : base(data)
        {
            this.userManager = new ApplicationUserManager(new UserStore<User>(new TicketingSystemSystemDbContext()));
        }
    }
}