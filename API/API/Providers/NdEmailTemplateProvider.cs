using RootSolutions.Common.Logger;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;

namespace RootSolutions.NetDiet.Therapist.API.Providers
{
    public static class NdEmailTemplateProvider
    {
        #region Private Fields --------------------------------------
        private static ILogger _logger = new DefaultLogger();
        #endregion --------------------------------------------------

        #region Private Methods -------------------------------------
        private static string CreateEmailBody(string templateName, Dictionary<string, string> templateData = null)
        {
            string content = "";

            try
            {
                content = File.ReadAllText(HttpContext.Current.Server.MapPath(Path.Combine("~/EmailTemplates", templateName)));
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Error reading template [tamplateName: {0}]", templateName), ex);
                throw ex;
            }

            if (templateData != null && templateData.Any())
            {
                foreach (var data in templateData)
                {
                    content = content.Replace(string.Format("{{{{{0}}}}}", data.Key), data.Value);
                }
            }

            return content;
        }
        #endregion --------------------------------------------------

        #region Public Methods --------------------------------------
        public static string CreateConfirmEmailWithPassword(string callbackUrl, string password)
        {
            var body = CreateEmailBody(ConfigurationManager.AppSettings["emailTemplates:ConfirmEmailWithPassword"], new Dictionary<string, string>() { { "callbackUrl", callbackUrl }, { "password", password } });

            return body;
        }
        #endregion --------------------------------------------------
    }
}