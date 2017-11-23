using log4net;
using System;
using System.Runtime.CompilerServices;

namespace RS.NetDiet.Therapist.DataModel
{
    public interface ILogger
    {
        void Debug(string message, [CallerMemberName] string method = null);

        void Info(string message, [CallerMemberName] string method = null);

        void Warning(string message, [CallerMemberName] string method = null);

        void Error(string message, Exception ex = null, [CallerMemberName] string method = null);
    }
}
