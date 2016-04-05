namespace TicketingSystem.Data
{
    using System.Data.Entity;

    using Microsoft.AspNet.Identity.EntityFramework;

    using Models;

    using TicketingSystem.Data.Migrations;

    public class TicketingSystemSystemDbContext : IdentityDbContext<User>, ITicketingSystemDbContext
    {
        public IDbSet<Ticket> Tickets { get; set; }

        public IDbSet<Reply> Replies { get; set; }

        public IDbSet<UserSession> UserSessions { get; set; }

        public TicketingSystemSystemDbContext()
            : base("name=TicketingSystem")
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<TicketingSystemSystemDbContext, Configuration>());
        }

        public static TicketingSystemSystemDbContext Create()
        {
            return new TicketingSystemSystemDbContext();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Ticket>()
                .HasRequired<User>(t => t.Creator)
                .WithMany(u => u.Tickets)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Reply>()
                .HasRequired<Ticket>(r => r.Ticket)
                .WithMany(t => t.Replies)
                .WillCascadeOnDelete(false);

            base.OnModelCreating(modelBuilder);
        }
    }
}
