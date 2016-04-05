namespace TicketingSystem.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class Ticket
    {
        private ICollection<Reply> replies;

        public Ticket()
        {
            this.replies = new HashSet<Reply>();
        }

        [Key]
        public int Id { get; set; }

        [MinLength(10), MaxLength(100)]
        public string Title { get; set; }

        [MinLength(10)]
        [Required]
        public string Content { get; set; }

        public TicketState State { get; set; }

        public string CreatorId { get; set; }

        public virtual User Creator { get; set; }

        public string AssigneeId { get; set; }

        public virtual User Assignee { get; set; }

        public DateTime PublishedAt { get; set; }

        public virtual ICollection<Reply> Replies
        {
            get
            {
                return this.replies;
            }
            set
            {
                this.replies = value;
            }
        }
    }
}
