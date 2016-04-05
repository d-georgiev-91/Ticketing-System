namespace TicketingSystem.Data
{
    using TicketingSystem.Data.Repositories;
    using TicketingSystem.Models;

    public interface ITicketingSystemData
    {
        IRepository<Ticket> Tickets { get; }

        IRepository<Reply> Replies { get; }

        IRepository<User> Users { get; }

        IRepository<UserSession> UserSessions { get; }

        int SaveChanges();
    }
}
