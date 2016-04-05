namespace TicketingSystem.Data
{
    using System.Data.Entity;

    using TicketingSystem.Models;

    public interface ITicketingSystemDbContext
    {
        IDbSet<Ticket> Tickets { get; set; }

        IDbSet<Reply> Replies { get; set; }

        IDbSet<User> Users { get; set; }

        IDbSet<UserSession> UserSessions { get; set; }

        int SaveChanges();
    }
}