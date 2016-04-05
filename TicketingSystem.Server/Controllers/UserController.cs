namespace TicketingSystem.Server.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web.Http;

    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using Microsoft.Owin.Security;

    using Newtonsoft.Json;

    using TicketingSystem.Data;
    using TicketingSystem.Models;
    using TicketingSystem.Server.Models.Tickets;
    using TicketingSystem.Server.Models.User;
    using TicketingSystem.Server.Properties;
    using TicketingSystem.Server.Utils;

    /// <summary>
    /// Controller that handles user interactions
    /// </summary>
    [SessionAuthorize]
    [RoutePrefix("api/user")]
    public class UserController : BaseApiController
    {
        private ApplicationUserManager userManager;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        public UserController(ITicketingSystemData data) :
            base(data)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        public UserController()
            : base(new TicketingSystemData())
        {
            this.userManager = new ApplicationUserManager(new UserStore<User>(new TicketingSystemSystemDbContext()));
        }

        public ApplicationUserManager UserManager => this.userManager;

        private IAuthenticationManager Authentication => this.Request.GetOwinContext().Authentication;

        /// <summary>
        /// Action method for user creation
        /// </summary>
        /// <param name="model">Accepts <see cref="RegisterUserBindingModel"/> </param>
        /// <returns>
        /// If request and data is valid a JSON is returned
        /// <example>
        /// {   
        ///     "access_token": " &lt;token&gt;",
        ///     "token_type": "bearer",
        ///     "expires_in":  &lt;time&gt;,
        ///     "username": " &lt;username&gt;",
        ///     ".issued": "&lt;date &amp; time token is created&gt;",
        ///     ".expires": " &lt;date &amp; time token is expire&gt;"
        /// }
        /// </example>
        /// else BadRequest reponse is returned
        /// </returns>
        [HttpPost]
        [AllowAnonymous]
        [Route("Register")]
        public async Task<IHttpActionResult> RegisterUser(RegisterUserBindingModel model)
        {
            if (model == null)
            {
                return this.BadRequest("Invalid user data");
            }

            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            var user = new User
            {
                UserName = model.Username,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber
            };

            var identityResult = await this.UserManager.CreateAsync(user, model.Password);

            if (!identityResult.Succeeded)
            {
                return this.GetErrorResult(identityResult);
            }

            var loginResult = await this.LoginUser(new LoginUserBindingModel
            {
                Username = model.Username,
                Password = model.Password
            });

            return loginResult;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [Route("Login")]
        public async Task<IHttpActionResult> LoginUser(LoginUserBindingModel model)
        {
            if (model == null)
            {
                return this.BadRequest("Invalid user data");
            }

            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            // Invoke the "token" OWIN service to perform the login (POST /api/token)
            // Use Microsoft.Owin.Testing.TestServer to perform in-memory HTTP POST request
            var testServer = Microsoft.Owin.Testing.TestServer.Create<Startup>();
            var requestParams = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("username", model.Username),
                new KeyValuePair<string, string>("password", model.Password)
            };
            var requestParamsFormUrlEncoded = new FormUrlEncodedContent(requestParams);
            var tokenServiceResponse = await testServer.HttpClient.PostAsync(Startup.TokenEndpointPath, requestParamsFormUrlEncoded);

            if (tokenServiceResponse.StatusCode == HttpStatusCode.OK)
            {
                // Sucessful login --> create user session in the database
                var responseString = await tokenServiceResponse.Content.ReadAsStringAsync();
                var responseData = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseString);
                var authToken = responseData["access_token"];
                var username = responseData["username"];
                var userSessionManager = new UserSessionManager();
                userSessionManager.CreateUserSession(username, authToken);

                // Cleanup: delete expired sessions fromthe database
                userSessionManager.DeleteExpiredSessions();
            }

            return this.ResponseMessage(tokenServiceResponse);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [SessionAuthorize]
        [Route("Logout")]
        public IHttpActionResult Logout()
        {
            // This does not actually perform logout! The OWIN OAuth implementation
            // does not support "revoke OAuth token" (logout) by design.
            this.Authentication.SignOut(DefaultAuthenticationTypes.ExternalBearer);

            // Delete the user's session from the database (revoke its bearer token)
            var userSessionManager = new UserSessionManager();
            userSessionManager.InvalidateUserSession();

            return this.Ok(
                new
                {
                    message = "Logout successful."
                }
            );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("ChangePassword")]
        public async Task<IHttpActionResult> ChangeUserPassword(ChangePasswordBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            if (User.Identity.GetUserName() == "admin")
            {
                return this.BadRequest("Password change for user 'admin' is not allowed!");
            }

            IdentityResult result = await this.UserManager.ChangePasswordAsync(
                User.Identity.GetUserId(), model.OldPassword, model.NewPassword);

            if (!result.Succeeded)
            {
                return this.GetErrorResult(result);
            }

            return this.Ok(
                new
                {
                    message = "Password changed successfully.",
                }
            );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("Profile")]
        public IHttpActionResult GetUserProfile()
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            // Validate the current user exists in the database
            var currentUserId = this.User.Identity.GetUserId();
            var currentUser = this.Data.Users.All().FirstOrDefault(x => x.Id == currentUserId);
            if (currentUser == null)
            {
                return this.BadRequest("Invalid user token! Please login again!");
            }

            var userToReturn = new
            {
                currentUser.FirstName,
                currentUser.LastName,
                currentUser.Email,
                currentUser.PhoneNumber,
            };

            return this.Ok(userToReturn);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("Profile")]
        public IHttpActionResult EditUserProfile(EditUserProfileBindingModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            // Validate the current user exists in the database
            var currentUserId = User.Identity.GetUserId();
            var currentUser = this.Data.Users.All().FirstOrDefault(x => x.Id == currentUserId);
            if (currentUser == null)
            {
                return this.BadRequest("Invalid user token! Please login again!");
            }

            if (this.User.Identity.GetUserName() == "admin")
            {
                return this.BadRequest("Edit profile for user 'admin' is not allowed!");
            }

            var hasEmailTaken = this.Data.Users.All().Any(x => x.Email == model.Email);
            if (hasEmailTaken)
            {
                return this.BadRequest("Invalid email. The email is already taken!");
            }

            currentUser.FirstName = model.FirstName;
            currentUser.LastName = model.LastName;
            currentUser.Email = model.Email;
            currentUser.PhoneNumber = model.PhoneNumber;

            this.Data.SaveChanges();

            return this.Ok(
                new
                {
                    message = "User profile edited successfully.",
                });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Tickets")]
        public IHttpActionResult CreateNewTicket(UserCreateTicketBindingModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            var currentUserId = User.Identity.GetUserId();
            var currentUser = this.Data.Users.All().FirstOrDefault(u => u.Id == currentUserId);

            if (currentUser == null)
            {
                return this.BadRequest("Invalid user token! Please login again!");
            }

            var ticket = new Ticket
            {
                Title = model.Title,
                Content = model.Content,
                CreatorId = currentUserId,
                State = TicketState.New,
                PublishedAt = DateTime.Now
            };

            this.Data.Tickets.Add(ticket);
            this.Data.SaveChanges();

            //TODO should return 201 Created
            return this.Ok(
                new
                {
                    message = "Ticket created successfully.",
                    ticketId = ticket.Id
                }
            );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("Tickets")]
        public IHttpActionResult GetTickets([FromUri]GetUserTicketsBindingModel model)
        {
            if (model == null)
            {
                model = new GetUserTicketsBindingModel();
            }

            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            var currentUserId = this.User.Identity.GetUserId();
            var currentUser = this.Data.Users.All().FirstOrDefault(u => u.Id == currentUserId);

            if (currentUser == null)
            {
                return this.BadRequest("Invalud user toke! Please login again!");
            }

            var tickets = this.Data.Tickets.All()
                .Include(t => t.Assignee);

            if (model.State.HasValue)
            {
                tickets = tickets.Where(t => t.State == model.State.Value);
            }

            tickets = tickets.Where(t => t.CreatorId == currentUserId);
            tickets = tickets.OrderByDescending(t => t.PublishedAt).ThenBy(t => t.Id);

            int pageSize = Settings.Default.DefaultPageSize;

            if (model.PageSize.HasValue)
            {
                pageSize = model.PageSize.Value;
            }

            int ticketsCount = tickets.Count();
            int pagesCount = (ticketsCount + pageSize - 1) / pageSize;

            if (model.StartPage.HasValue)
            {
                tickets = tickets.Skip(pageSize * (model.StartPage.Value - 1));
            }

            tickets = tickets.Take(pageSize);

            var ticketsToReturn = tickets
                .ToList()
                .Select(t => new
                {
                    id = t.Id,
                    title = t.Title,
                    content = t.Content,
                    publishedAt = t.PublishedAt,
                    state = t.State.ToString(),
                    assignee = t.Assignee == null ? null : new
                    {
                        firstName = t.Assignee.FirstName,
                        lastName = t.Assignee.LastName
                    }
                });

            return this.Ok(new
            {
                ticketsCount,
                pagesCount,
                tickets = ticketsToReturn
            });
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
                .FirstOrDefault(t => t.Id == ticketId);

            if (ticket == null)
            {
                return this.NotFound();
            }

            var currentUserId = this.User.Identity.GetUserId();

            if (ticket.CreatorId != currentUserId)
            {
                return this.Unauthorized();
            }

            return this.Ok(new
            {
                id = ticket.Id,
                title = ticket.Title,
                state = ticket.State,
                publishedAt = ticket.PublishedAt,
                assignee = ticket.Assignee == null ? null : new
                {
                    firstName = ticket.Assignee.FirstName,
                    lastName = ticket.Assignee.LastName
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
        public IHttpActionResult ReplyToTicket(int ticketId, UserReplyBindingModel model)
        {
            var ticket = this.Data.Tickets.All()
                .Include(t => t.Assignee)
                .FirstOrDefault(t => t.Id == ticketId);

            if (ticket == null)
            {
                return this.NotFound();
            }

            var currentUserId = this.User.Identity.GetUserId();

            if (ticket.CreatorId != currentUserId)
            {
                return this.Unauthorized();
            }

            ticket.Replies.Add(new Reply
            {
                AuthorId = currentUserId,
                Content = model.Content,
                PublishedAt = DateTime.Now
            });

            if (ticket.Assignee != null)
            {
                ticket.State = TicketState.InProgress;
            }

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
        /// <returns></returns>
        [HttpPut]
        [Route("Tickets/Close/{id:int}")]
        public IHttpActionResult Close(int ticketId)
        {
            var ticket = this.Data.Tickets.All().FirstOrDefault(t => t.Id == ticketId);

            if (ticket == null)
            {
                return this.BadRequest($"Ticket ${ticketId} not found!");
            }

            var currentUserId = User.Identity.GetUserId();

            if (ticket.CreatorId != currentUserId)
            {
                return this.Unauthorized();
            }

            ticket.State = TicketState.Closed;
            this.Data.SaveChanges();

            return this.Ok(new { message = $"Ticket ${ticketId} has been closed." });
        }

        /// <summary>
        /// Releases the unmanaged resources that are used by the object and, optionally, releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.UserManager.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
