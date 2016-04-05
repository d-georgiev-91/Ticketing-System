namespace TicketingSystem.Server.Models.Tickets
{
    using TicketingSystem.Models;

    /// <summary>
    /// Model holding 
    /// </summary>
    public class GetUserTicketsBindingModel
    {
        /// <summary>
        /// 
        /// </summary>
        public GetUserTicketsBindingModel()
        {
            this.StartPage = 1;
        }

        /// <summary>
        /// Ticket State
        /// </summary>
        public TicketState? State { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int? PageSize { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int? StartPage { get; set; }
    }
}