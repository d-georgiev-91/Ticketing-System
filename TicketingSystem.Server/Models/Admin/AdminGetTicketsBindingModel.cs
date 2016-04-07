namespace TicketingSystem.Server.Models.Admin
{
    using System;
    using global::TicketingSystem.Models;
    using System.Linq.Expressions;

    using TicketingSystem.Models;

    /// <summary>
    /// 
    /// </summary>
    public class AdminGetTicketsBindingModel
    {
        public AdminGetTicketsBindingModel()
        {
            this.StartPage = 1;
        }

        /// <summary>
        /// 
        /// </summary>
        public TicketState? State { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string AssigneeId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string SortBy { get; set; }

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