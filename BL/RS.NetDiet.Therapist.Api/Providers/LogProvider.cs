using log4net;
using RS.NetDiet.Therapist.DataModel;
using System;
using System.Runtime.CompilerServices;

namespace RS.NetDiet.Therapist.Api.Providers
{
    public class LogProvider : ILogger
    {
        private static ILog _logger;
        private const string ND_LOGGER_NAME = "NdLogger";
        private const string LOG_FORMAT_WITH_CALLER_METHOD = "<{0}>. {1}";

        public LogProvider()
        {
            log4net.Config.XmlConfigurator.Configure();
            _logger = LogManager.GetLogger(ND_LOGGER_NAME);
        }

        public void Debug(string message, [CallerMemberName] string method = null)
        {
            if (!string.IsNullOrWhiteSpace(method))
            {
                message = string.Format(LOG_FORMAT_WITH_CALLER_METHOD, method, message);
            }

            _logger.Debug(message);
        }

        public void Info(string message, [CallerMemberName] string method = null)
        {
            if (!string.IsNullOrWhiteSpace(method))
            {
                message = string.Format(LOG_FORMAT_WITH_CALLER_METHOD, method, message);
            }

            _logger.Info(message);
        }

        public void Warning(string message, [CallerMemberName] string method = null)
        {
            if (!string.IsNullOrWhiteSpace(method))
            {
                message = string.Format(LOG_FORMAT_WITH_CALLER_METHOD, method, message);
            }

            _logger.Warn(message);
        }

        public void Error(string message, Exception ex = null, [CallerMemberName] string method = null)
        {
            if (!string.IsNullOrWhiteSpace(method))
            {
                message = string.Format(LOG_FORMAT_WITH_CALLER_METHOD, method, message);
            }

            if (ex == null)
            {
                _logger.Error(message);
            }
            else
            {
                _logger.Error(message, ex);
            }
        }
    }
}