﻿namespace TicketingSystem.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Security.Claims;
    using System.Threading.Tasks;

    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;

    public class User : IdentityUser
    {
        private ICollection<Ticket> tickets;

        public User()
        {
            this.tickets = new HashSet<Ticket>();
        }


        [MinLength(3)]
        [Required]
        public string FirstName { get; set; }

        [MinLength(3)]
        [Required]
        public string LastName { get; set; }

        public virtual ICollection<Ticket> Tickets
        {
            get
            {
                return this.tickets;
            }
            set
            {
                this.tickets = value;
            }
        }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(
           UserManager<User> manager, string authenticationType)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, authenticationType);

            // Add custom user claims here
            return userIdentity;
        }
    }
}