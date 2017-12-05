using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using RootSolutions.Common.Logger;
using RootSolutions.Common.Web.Infrastructure;
using RootSolutions.Common.Web.Providers;
using RootSolutions.NetDiet.Therapist.API.Infrastructure;
using RootSolutions.NetDiet.Therapist.API.Services;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RootSolutions.NetDiet.Therapist.API.Providers
{
    public class NdOAuthProvider : RsOAuthProvider<NdUser, RsUserManagerWithMessageService<NdUser, NdDbContext, NdEmailService>>
    {
        public NdOAuthProvider(ILogger logger, params string[] allowedOrigins) : base(logger, allowedOrigins)
        {
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", _allowedOrigins);

            var userManager = context.OwinContext.GetUserManager<RsUserManagerWithMessageService<NdUser, NdDbContext, NdEmailService>>();
            var user = await userManager.FindAsync(context.UserName, context.Password);

            if (user == null)
            {
                context.SetError("invalid_grant", "The user name or password is incorrect.");
                _logger.Debug(string.Format("Invalid login for user [{0}]", context.UserName));
                return;
            }

            if (!user.EmailConfirmed)
            {
                context.SetError("confirm_email", "User did not confirm email.");
                _logger.Debug(string.Format("User [{0}] did not confirm email", context.UserName));
                return;
            }

            if (user.MustChangePassword)
            {
                context.SetError("change_password", "User did not change password.");
                _logger.Debug(string.Format("User [{0}] did not change password", context.UserName));
                return;
            }

            ClaimsIdentity oAuthIdentity = await userManager.CreateIdentityAsync(user, "JWT");
            AuthenticationProperties properties = new AuthenticationProperties(new Dictionary<string, string>());
            var ticket = new AuthenticationTicket(oAuthIdentity, properties);

            context.Validated(ticket);
        }
    }
}