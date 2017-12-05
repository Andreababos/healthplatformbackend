using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.DataHandler.Encoder;
using Microsoft.Owin.Security.Jwt;
using Microsoft.Owin.Security.OAuth;
using Newtonsoft.Json.Serialization;
using Owin;
using RootSolutions.Common.Logger;
using RootSolutions.Common.Web.Infrastructure;
using RootSolutions.Common.Web.Providers;
using RootSolutions.NetDiet.Therapist.API.Infrastructure;
using RootSolutions.NetDiet.Therapist.API.Providers;
using RootSolutions.NetDiet.Therapist.API.Services;
using System;
using System.Configuration;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web.Http;

namespace RootSolutions.NetDiet.Therapist.API
{
    public class Startup
    {
        #region Public Methods --------------------------------------
        public void Configuration(IAppBuilder app)
        {
            HttpConfiguration httpConfig = new HttpConfiguration();

            ConfigureOAuthTokenGeneration(app);
            ConfigureOAuthTokenConsumption(app);
            ConfigureWebApi(httpConfig);

            app.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);
            app.UseWebApi(httpConfig);
        }
        #endregion --------------------------------------------------

        #region Private Methods -------------------------------------
        private void ConfigureOAuthTokenGeneration(IAppBuilder app)
        {
            app.CreatePerOwinContext(NdDbContext.Create);
            app.CreatePerOwinContext<RsUserManagerWithMessageService<NdUser, NdDbContext, NdEmailService>>(
                RsUserManagerWithMessageService<NdUser, NdDbContext, NdEmailService>.Create);
            app.CreatePerOwinContext<RsRoleManager<NdUser, NdDbContext>>(RsRoleManager<NdUser, NdDbContext>.Create);

            OAuthAuthorizationServerOptions OAuthServerOptions = new OAuthAuthorizationServerOptions()
            {
                AllowInsecureHttp = true,
                TokenEndpointPath = new PathString("/oauth/token"),
                AccessTokenExpireTimeSpan = TimeSpan.FromDays(1),
                Provider = new NdOAuthProvider(new DefaultLogger()),
#if DEBUG
                AccessTokenFormat = new RsJwtFormatProvider("http://localhost/NetDiet/Therapist")
#else
                AccessTokenFormat = new NdJwtFormat("http://mintest.dk/")
#endif
            };

            app.UseOAuthAuthorizationServer(OAuthServerOptions);
        }

        private void ConfigureOAuthTokenConsumption(IAppBuilder app)
        {
#if DEBUG
            var issuer = "http://localhost/NetDiet/Therapist";
#else
            var issuer = "http://mintest.dk/";
#endif
            string audienceId = ConfigurationManager.AppSettings["as:AudienceId"];
            byte[] audienceSecret = TextEncodings.Base64Url.Decode(ConfigurationManager.AppSettings["as:AudienceSecret"]);

            app.UseJwtBearerAuthentication(
                new JwtBearerAuthenticationOptions
                {
                    AuthenticationMode = AuthenticationMode.Active,
                    AllowedAudiences = new[] { audienceId },
                    IssuerSecurityTokenProviders = new IIssuerSecurityTokenProvider[]
                    {
                        new SymmetricKeyIssuerSecurityTokenProvider(issuer, audienceSecret)
                    }
                });
        }

        private void ConfigureWebApi(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();

            var jsonFormatter = config.Formatters.OfType<JsonMediaTypeFormatter>().First();
            jsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        }
        #endregion --------------------------------------------------
    }
}