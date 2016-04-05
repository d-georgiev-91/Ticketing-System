namespace TicketingSystem.Data
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;

    using TicketingSystem.Data.Repositories;
    using TicketingSystem.Models;

    public class TicketingSystemData : ITicketingSystemData
    {
        private readonly ITicketingSystemDbContext context;
        private readonly IDictionary<Type, object> repositories;

        public TicketingSystemData() : 
            this(new TicketingSystemSystemDbContext())
        {

        }

        public TicketingSystemData(ITicketingSystemDbContext context)
        {
            this.context = context;
            this.repositories = new Dictionary<Type, object>();
        }

        IRepository<Ticket> ITicketingSystemData.Tickets => this.GetRepository<Ticket>();

        IRepository<Reply> ITicketingSystemData.Replies => this.GetRepository<Reply>();

        IRepository<User> ITicketingSystemData.Users => this.GetRepository<User>();

        IRepository<UserSession> ITicketingSystemData.UserSessions => this.GetRepository<UserSession>();

        public int SaveChanges()
        {
            return this.context.SaveChanges();
        }

        private IRepository<T> GetRepository<T>() where T : class
        {
            var type = typeof(T);

            if (!this.repositories.ContainsKey(type))
            {
                var typeOfRepository = typeof(GenericRepository<T>);

                var repository = Activator.CreateInstance(typeOfRepository, this.context);
                this.repositories.Add(type, repository);
            }

            return (IRepository<T>)this.repositories[type];
        }
    }
}
