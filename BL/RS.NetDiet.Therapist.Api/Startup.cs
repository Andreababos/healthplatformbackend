using Newtonsoft.Json.Serialization;
using Owin;
using RS.NetDiet.Therapist.Api.Infrastructure;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web.Http;

namespace RS.NetDiet.Therapist.Api
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            HttpConfiguration httpConfig = new HttpConfiguration();

            ConfigureOAuthTokenGeneration(app);
            ConfigureWebApi(httpConfig);

            app.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);
            app.UseWebApi(httpConfig);

        }

        private void ConfigureOAuthTokenGeneration(IAppBuilder app)
        {
            app.CreatePerOwinContext(NdDbContext.Create);
            app.CreatePerOwinContext<NdUserManager>(NdUserManager.Create);

            // Plugin the OAuth bearer JSON Web Token tokens generation and Consumption will be here

        }

        private void ConfigureWebApi(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();

            var jsonFormatter = config.Formatters.OfType<JsonMediaTypeFormatter>().First();
            jsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        }
    }
}