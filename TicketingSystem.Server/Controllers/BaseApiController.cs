namespace TicketingSystem.Server.Controllers
{
    using System.Web.Http;

    using Microsoft.AspNet.Identity;

    using TicketingSystem.Data;

    public class BaseApiController: ApiController
    {
        protected ITicketingSystemData Data { get; private set; }

        public BaseApiController(ITicketingSystemData data)
        {
            this.Data = data;
        }

        protected IHttpActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null)
            {
                return this.InternalServerError();
            }

            if (!result.Succeeded)
            {
                if (result.Errors != null)
                {
                    foreach (string error in result.Errors)
                    {
                        this.ModelState.AddModelError(string.Empty, error);
                    }
                }

                if (this.ModelState.IsValid)
                {
                    // No ModelState errors are available to send, so just return an empty BadRequest.
                    return this.BadRequest();
                }

                return this.BadRequest(this.ModelState);
            }

            return null;
        }
    }
            
}