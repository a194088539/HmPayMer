using log4net;
using System;

namespace HM.Framework.Logging
{
    public class LogUtil
    {
        private static readonly ILog log_debug = LogManager.GetLogger("logdebug");

        private static readonly ILog log_info = LogManager.GetLogger("loginfo");

        private static readonly ILog log_error = LogManager.GetLogger("logerror");

        public static void Debug(object message)
        {
            log_debug.Debug(message);
        }

        public static void Debug(object message, Exception exception)
        {
            log_debug.Debug(message, exception);
        }

        public static void DebugFormat(IFormatProvider provider, string format, params object[] args)
        {
            log_debug.DebugFormat(provider, format, args);
        }

        public static void DebugFormat(string format, params object[] args)
        {
            log_debug.DebugFormat(format, args);
        }

        public static void DebugFormat(string format, object arg0)
        {
            log_debug.DebugFormat(format, arg0);
        }

        public static void DebugFormat(string format, object arg0, object arg1)
        {
            log_debug.DebugFormat(format, arg0, arg1);
        }

        public static void DebugFormat(string format, object arg0, object arg1, object arg2)
        {
            log_debug.DebugFormat(format, arg0, arg1, arg2);
        }

        public static void Info(object message)
        {
            log_info.Info(message);
        }

        public static void Info(object message, Exception exception)
        {
            log_info.Info(message, exception);
        }

        public static void InfoFormat(string format, object arg0)
        {
            log_info.InfoFormat(format, arg0);
        }

        public static void InfoFormat(string format, object arg0, object arg1)
        {
            log_info.InfoFormat(format, arg0, arg1);
        }

        public static void InfoFormat(string format, object arg0, object arg1, object arg2)
        {
            log_info.InfoFormat(format, arg0, arg1, arg2);
        }

        public static void InfoFormat(string format, params object[] args)
        {
            log_info.InfoFormat(format, args);
        }

        public static void InfoFormat(IFormatProvider provider, string format, params object[] args)
        {
            log_info.InfoFormat(provider, format, args);
        }

        public static void Error(object message)
        {
            log_error.Error(message);
        }

        public static void Error(object message, Exception exception)
        {
            log_error.Error(message, exception);
        }

        public static void ErrorFormat(string format, object arg0)
        {
            log_error.ErrorFormat(format, arg0);
        }

        public static void ErrorFormat(string format, object arg0, object arg1)
        {
            log_error.ErrorFormat(format, arg0, arg1);
        }

        public static void ErrorFormat(string format, object arg0, object arg1, object arg2)
        {
            log_error.ErrorFormat(format, arg0, arg1, arg2);
        }

        public static void ErrorFormat(string format, params object[] args)
        {
            log_error.ErrorFormat(format, args);
        }

        public static void ErrorFormat(IFormatProvider provider, string format, params object[] args)
        {
            log_error.ErrorFormat(provider, format, args);
        }
    }
}
