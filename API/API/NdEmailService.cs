using RootSolutions.Common.Logger;
using RootSolutions.Common.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace RootSolutions.NetDiet.Therapist.API
{
    public class NdEmailService : EmailService
    {
        #region Constructors ----------------------------------------
        public NdEmailService() : base(new DefaultLogger())
        { }
        #endregion --------------------------------------------------

        #region Private Methods -------------------------------------
        private string CreateEmailBody(string templateName, Dictionary<string, string> templateData = null)
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
    }
}