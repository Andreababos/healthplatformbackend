﻿using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using RS.NetDiet.Therapist.Api.Infrastructure;
using RS.NetDiet.Therapist.DataModel;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RS.NetDiet.Therapist.Api.Providers
{
    public class NdOAuthProvider : OAuthAuthorizationServerProvider
    {
        private ILogger _logger = new LogProvider();

        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            context.Validated();
            return Task.FromResult<object>(null);
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            var allowedOrigin = "*";
            context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { allowedOrigin });

            var userManager = context.OwinContext.GetUserManager<NdUserManager>();
            NdUser user = await userManager.FindByEmailAsync(context.UserName);
            if (user != null)
            {
                user = await userManager.FindAsync(user.UserName, context.Password);
            }
            else
            {
                user = await userManager.FindAsync(context.UserName, context.Password);
            }
            

            if (user == null)
            {
                context.SetError("invalid_grant", "The user name or password is incorrect.");
                _logger.Debug(string.Format("The user name or password is incorrect [username: {0}, password: {1}]", context.UserName, context.Password));
                return;
            }

            if (!user.EmailConfirmed)
            {
                context.SetError("invalid_grant", "User did not confirm email.");
                _logger.Debug(string.Format("User did not confirm email [username: {0}]", context.UserName));
                return;
            }

            ClaimsIdentity oAuthIdentity = await user.GenerateUserIdentityAsync(userManager, "JWT");
            AuthenticationProperties properties = CreateProperties(user);
            var ticket = new AuthenticationTicket(oAuthIdentity, properties);

            context.Validated(ticket);

            _logger.Debug(string.Format("User logged in [username: {0}, password: {1}]", context.UserName, context.Password));
        }

        public override Task TokenEndpoint(OAuthTokenEndpointContext context)
        {
            foreach (KeyValuePair<string, string> property in context.Properties.Dictionary)
            {
                context.AdditionalResponseParameters.Add(property.Key, property.Value);
            }

            return Task.FromResult<object>(null);
        }

        public static AuthenticationProperties CreateProperties(NdUser user)
        {
            IDictionary<string, string> data = new Dictionary<string, string>
            {
                { "email", user.Email }
            };
            return new AuthenticationProperties(data);
        }
    }
}