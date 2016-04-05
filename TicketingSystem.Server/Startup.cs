using Microsoft.Owin;

[assembly: OwinStartup(typeof(TicketingSystem.Server.Startup))]

namespace TicketingSystem.Server
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Owin;
    using Microsoft.Owin.Cors;
    using System.Web.Cors;
    using System.Threading.Tasks;
    using System.Web.Http;

    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseCors(new CorsOptions()
            {
                PolicyProvider = new CorsPolicyProvider()
                {
                    PolicyResolver = request =>
                    {
                        if (request.Path.StartsWithSegments(new PathString(TokenEndpointPath)))
                        {
                            return Task.FromResult(new CorsPolicy { AllowAnyOrigin = true });
                        }

                        return Task.FromResult<CorsPolicy>(null);
                    }
                }
            });

            ConfigureAuth(app);

            GlobalConfiguration.Configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
        }
    }
}
