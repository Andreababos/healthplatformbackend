using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using System.Configuration;
using System.Net.Mail;
using System.Text;
using System.ComponentModel;
using RS.NetDiet.Therapist.DataModel;
using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;

namespace RS.NetDiet.Therapist.Api.Services
{
    public class NdEmailService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            var email = new MailMessage(
                new MailAddress(ConfigurationManager.AppSettings["emailService:Account"], ConfigurationManager.AppSettings["emailService:DisplayName"], Encoding.UTF8),
                new MailAddress(message.Destination))
            {
                Subject = message.Subject,
                SubjectEncoding = Encoding.UTF8,
                Body = message.Body,
                BodyEncoding = Encoding.UTF8,
                IsBodyHtml = true,
            };

            var client = new SmtpClient(ConfigurationManager.AppSettings["emailService:Host"], Convert.ToInt32(ConfigurationManager.AppSettings["emailService:Port"]));
            client.Credentials = new NetworkCredential(ConfigurationManager.AppSettings["emailService:Account"], ConfigurationManager.AppSettings["emailService:Password"]);
            client.SendCompleted += new SendCompletedEventHandler(SendCompletedCallback);

            return client.SendMailAsync(email);
        }

        private void SendCompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                NdLogger.Error("Error sending e-mail", e.Error);
            }
            else
            {
                NdLogger.Debug("E-mail sent successfully");
            }

            (sender as SmtpClient).Dispose();
        }

        public static string CreateConfirmEmailBody(string callbackUrl)
        {
            var body = CreateEmailBody(ConfigurationManager.AppSettings["emailTemplates:ConfirmEmail"], new Dictionary<string, string>() { { "callbackUrl", callbackUrl } });

            return body;
        }

        public static string CreateConfirmEmailWithPasswordBody(string callbackUrl, string password)
        {
            var body = CreateEmailBody(ConfigurationManager.AppSettings["emailTemplates:ConfirmEmailWithPassword"], new Dictionary<string, string>() { { "callbackUrl", callbackUrl }, { "password", password } });

            return body;
        }

        public static string CreateResetPasswordBody(string callbackUrl)
        {
            var body = CreateEmailBody(ConfigurationManager.AppSettings["emailTemplates:ResetPassword"], new Dictionary<string, string>() { { "callbackUrl", callbackUrl } });

            return body;
        }

        private static string CreateEmailBody(string templateName, Dictionary<string, string> templateData = null)
        {
            string content = "";

            try
            {
                content = File.ReadAllText(HttpContext.Current.Server.MapPath(Path.Combine("~/EmailTemplates", templateName)));
            }
            catch (Exception ex)
            {
                NdLogger.Error(string.Format("Error reading template [tamplateName: {0}]", templateName), ex);
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
    }
}