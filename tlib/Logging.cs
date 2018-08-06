using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using log4net.Config;
using System.Data;
using TLib.Exceptions;
using System.IO;

namespace TLib.Logging
{
    public enum LogLevel
    {
        DEBUG = 1,
        ERROR,
        FATAL,
        INFO,
        WARN
    }

    public static class Logger
    {
        #region Members
        private static readonly ILog logger = LogManager.GetLogger(typeof(Logger));

        #endregion

        #region Constructors
        static Logger()
        {
            XmlConfigurator.Configure();
        }

        #endregion

        #region Methods

        public static void WriteLog(string msg, AbstractBaseException e)
        {
            if (null != e)
            {
                WriteLog(e.Level, "AbstractBaseException: {0}\r\n{1}", msg, e.StackTrace);
                WriteLog(e.InnerException);
            }
        }

        public static void WriteLog(AbstractBaseException e)
        {
            if (null != e)
            {
                WriteLog(e.Level, "AbstractBaseException: {0}\r\n{1}", e.Message, e.StackTrace);
                WriteLog(e.InnerException);
            }
        }

        public static void WriteLog(Exception e, string fmt, params object[] args)
        {
            Exception ex = new Exception(string.Format(fmt, args), e);
            WriteLog(ex);
        }

        public static void WriteLog(Exception e, LogLevel l = LogLevel.ERROR)
        {
            if (null != e)
            {
                WriteLog(l, string.Format("Message: {0}\r\nStackTrace:\r\n{1}", e.Message, e.StackTrace));
                WriteLog(e.InnerException);
            }
        }

        public static void WriteLine(string fmt, params object[] args)
        {
            Write(fmt, args);
        }
        public static void Write(string fmt, params object[] args)
        {
            WriteLog(LogLevel.INFO, fmt, args);
        }

        public static void WriteLog(LogLevel logLevel, string fmt, params object[] args)
        {
            string log = string.Format(fmt, args);
            if (logLevel.Equals(LogLevel.DEBUG))
            {
                logger.Debug(log);
            }
            else if (logLevel.Equals(LogLevel.ERROR))
            {
                logger.Error(log);
            }
            else if (logLevel.Equals(LogLevel.FATAL))
            {
                logger.Fatal(log);
            }
            else if (logLevel.Equals(LogLevel.INFO))
            {
                logger.Info(log);
            }
            else if (logLevel.Equals(LogLevel.WARN))
            {
                logger.Warn(log);
            }
        }
        #endregion
    }

    public class LogByDateFileAppender : log4net.Appender.RollingFileAppender
    {
        protected override void OpenFile(string fileName, bool append)
        {
            //Inject folder [yyyyMMdd] before the file name
            string baseDirectory = Path.GetDirectoryName(fileName);
            string fileNameOnly = Path.GetFileName(fileName) + DateTime.Now.ToString("ddMMyyHHss") + ".log";
            //string fileNameOnly = string.Format("{0}-{1}.log", Path.GetFileName(fileName), DateTime.Now.ToString("dd.MM.yy.HH:ss"));
            //string newDirectory = Path.Combine(baseDirectory, DateTime.Now.ToString("dd.MM.yy.HHss"));
            string newFileName = Path.Combine(baseDirectory, fileNameOnly);

            base.OpenFile(newFileName, append);
        }
    }

}
