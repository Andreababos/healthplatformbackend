using log4net;
using System;
using System.Runtime.CompilerServices;

namespace RS.NetDiet.Therapist.DataModel
{
    public static class NdLogger
    {
        private static ILog _logger;
        private const string ND_LOGGER_NAME = "NdLogger";
        private const string LOG_FORMAT_WITH_CALLER_METHOD = "<{0}>. {1}";

        public static void Configure()
        {
            log4net.Config.XmlConfigurator.Configure();
            _logger = LogManager.GetLogger(ND_LOGGER_NAME);
        }

        public static void Debug(string message, [CallerMemberName] string method = null)
        {
            if (!string.IsNullOrWhiteSpace(method))
            {
                message = string.Format(LOG_FORMAT_WITH_CALLER_METHOD, method, message);
            }

            _logger.Debug(message);
        }

        public static void Info(string message, [CallerMemberName] string method = null)
        {
            if (!string.IsNullOrWhiteSpace(method))
            {
                message = string.Format(LOG_FORMAT_WITH_CALLER_METHOD, method, message);
            }

            _logger.Info(message);
        }

        public static void Warning(string message, [CallerMemberName] string method = null)
        {
            if (!string.IsNullOrWhiteSpace(method))
            {
                message = string.Format(LOG_FORMAT_WITH_CALLER_METHOD, method, message);
            }

            _logger.Warn(message);
        }

        public static void Error(string message, Exception ex = null, [CallerMemberName] string method = null)
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
