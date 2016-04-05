namespace TicketingSystem.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class Reply
    {
        [Key]
        public int Id { get; set; }

        [MinLength(10)]
        public string Content { get; set; }

        public string AuthorId { get; set; }

        public virtual User Author { get; set; }

        public int TicketId { get; set; }

        public virtual Ticket Ticket { get; set; }

        public DateTime PublishedAt { get; set; }
    }
}