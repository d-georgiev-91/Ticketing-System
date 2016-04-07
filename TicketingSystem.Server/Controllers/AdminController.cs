namespace TicketingSystem.Server.Controllers
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Http;

    using Microsoft.Ajax.Utilities;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;

    using TicketingSystem.Data;
    using TicketingSystem.Models;
    using TicketingSystem.Server.Models.Admin;
    using TicketingSystem.Server.Properties;
    using TicketingSystem.Server.Utils;

    /// <summary>
    /// 
    /// </summary>
    [SessionAuthorize(Roles = "Administrator")]
    [Route("api/admin")]
    public class AdminController : BaseApiController
    {
        private ApplicationUserManager userManager;

        /// <summary>
        /// 
        /// </summary>
        public ApplicationUserManager UserManager
        {
            get
            {
                return this.userManager;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public AdminController()
            : this(new TicketingSystemData())
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        public AdminController(ITicketingSystemData data)
            : base(data)
        {
            this.userManager = new ApplicationUserManager(new UserStore<User>(new TicketingSystemSystemDbContext()));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("Tickets")]
        public IHttpActionResult GetTickets([FromUri]AdminGetTicketsBindingModel model)
        {
            if (model == null)
            {
                // When no parameters are passed, the model is null, so we create an empty model
                model = new AdminGetTicketsBindingModel();
            }

            // Validate the input parameters
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            var tickets = this.Data.Tickets.All();

            if (model.State.HasValue)
            {
                tickets = tickets.Where(t => t.State == model.State.Value);
            }

            if (!string.IsNullOrWhiteSpace(model.AssigneeId))
            {
                tickets = tickets.Where(t => t.AssigneeId == model.AssigneeId);
            }

            tickets = tickets.OrderByDescending(t => t.PublishedAt).ThenBy(t => t.Id);

            // Apply paging: find the requested page (by given start page and page size)
            int pageSize = Settings.Default.DefaultPageSize;
            if (model.PageSize.HasValue)
            {
                pageSize = model.PageSize.Value;
            }
            var ticketsCount = tickets.Count();
            var pagesCount = (ticketsCount + pageSize - 1) / pageSize;
            if (model.StartPage.HasValue)
            {
                tickets = tickets.Skip(pageSize * (model.StartPage.Value - 1));
            }
            tickets = tickets.Take(pageSize);

            // Select the columns to be returned 
            var ticketsToReturn = tickets.ToList().Select(t => new
            {
                id = t.Id,
                title = t.Title,
                publishedAt = t.PublishedAt.ToString("o"),
                creator = t.Creator == null ? null : new
                {
                    fistName = t.Creator.FirstName,
                    lastName = t.Creator.LastName,
                    username = t.Creator.UserName
                },
                assignee = t.Assignee == null ? null : new
                {
                    fistName = t.Assignee.FirstName,
                    lastName = t.Assignee.LastName,
                    username = t.Assignee.UserName
                },
                state = t.State.ToString()
            });

            return this.Ok(
                new
                {
                    ticketsCount,
                    pagesCount,
                    tickets = ticketsToReturn
                }
            );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ticketId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("Tickets/{id:int}")]
        public IHttpActionResult GetTicketById(int ticketId)
        {
            var ticket = this.Data.Tickets.All()
                .Include(t => t.Assignee)
                .Include(t => t.Creator)
                .FirstOrDefault(t => t.Id == ticketId);

            if (ticket == null)
            {
                return this.NotFound();
            }

            return this.Ok(new
            {
                id = ticket.Id,
                title = ticket.Title,
                state = ticket.State,
                publishedAt = ticket.PublishedAt,
                creator = ticket.Creator == null ? null : new
                {
                    firstName = ticket.Creator.FirstName,
                    lastName = ticket.Creator.LastName,
                    username = ticket.Creator.UserName,
                    email = ticket.Creator.Email
                },
                assignee = ticket.Assignee == null ? null : new
                {
                    firstName = ticket.Assignee.FirstName,
                    lastName = ticket.Assignee.LastName,
                    username = ticket.Assignee.UserName
                },
                replies = ticket.Replies
                    .OrderByDescending(r => r.PublishedAt)
                    .Select(r => new
                    {
                        id = r.Id,
                        author = new
                        {
                            id = r.AuthorId,
                            firstName = r.Author.FirstName,
                            lastName = r.Author.LastName
                        },
                        publishedAt = r.PublishedAt
                    })
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ticketId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Tickets/{id:int}/Reply")]
        public IHttpActionResult ReplyToTicket(int ticketId, [FromBody]AdminReplyBindingModel model)
        {
            var ticket = this.Data.Tickets.All()
                .Include(t => t.Assignee)
                .FirstOrDefault(t => t.Id == ticketId);

            if (ticket == null)
            {
                return this.NotFound();
            }

            var currentUserId = this.User.Identity.GetUserId();

            if (ticket.Assignee == null)
            {
                ticket.AssigneeId = currentUserId;
            }

            ticket.Replies.Add(new Reply
            {
                AuthorId = currentUserId,
                Content = model.Content,
                PublishedAt = DateTime.Now
            });

            ticket.State = TicketState.AwaitingReponse;

            this.Data.SaveChanges();

            return this.Ok(new
            {
                message = "Reply added successfuly."
            });
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ticketId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("Tickets/{ticketId:int}/Assign/{userId:string}")]
        public IHttpActionResult AssignTo(int ticketId, string userId)
        {
            var ticket = this.Data.Tickets.All()
                .FirstOrDefault(t => t.Id == ticketId);

            if (ticket == null)
            {
                return this.NotFound();
            }

            var user = this.Data.Users.All()
                .FirstOrDefault(u => u.Id == userId);

            if (user == null || !this.UserManager.IsInRole(userId,  "Administartor"))
            {
                this.BadRequest("Cannot assign ticket to user");
            }


            var currentUserId = this.User.Identity.GetUserId();

            if (ticket.Assignee == null)
            {
                ticket.AssigneeId = currentUserId;
            }

            ticket.State = TicketState.InProgress;

            this.Data.SaveChanges();

            return this.Ok(new
            {
                message = "Ticket assigned successfuly."
            });
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("Administrators")]
        public IHttpActionResult GetAdmins()
        {
            var administrators = this.Data.Users.All()
                .Where(u => this.UserManager.IsInRole(u.Id, "Administrator"))
                .Select(u => new
                {
                    id= u.Id,
                    username = u.UserName,
                    firstName = u.FirstName,
                    lastName = u.LastName               
                });

            return this.Ok(administrators);
        }
    }
}