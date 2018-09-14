using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Layout;
using log4net.Repository;
using System;
using System.Text;
using System.Xml;

namespace RoslyProject
{

    public class CCLog
    {
        private class SilentErrorHandler : IErrorHandler
        {
            private StringBuilder m_buffer = new StringBuilder();

            public string Message
            {
                get { return m_buffer.ToString(); }
            }

            public void Error(string message)
            {
                m_buffer.Append(message + "\n");
            }

            public void Error(string message, Exception e)
            {
                m_buffer.Append(message + "\n" + e.Message + "\n");
            }

            public void Error(string message, Exception e, ErrorCode errorCode)
            {
                m_buffer.Append(message + "\n" + e.Message + "\n");
            }
        }

        //static void InitXmlLog()
        //{
        //    XmlDocument log4netConfig = new XmlDocument();
        //    log4netConfig.LoadXml(@"
        //        <log4net>
        //          <appender name=""StringAppender"" type=""log4net.Tests.Appender.StringAppender, log4net.Tests"">
        //            <layout type=""log4net.Layout.SimpleLayout"" />
        //          </appender>
        //          <root>
        //            <level value=""ALL"" />
        //            <appender-ref ref=""StringAppender"" />
        //          </root>
        //          <logger name=""."">
        //            <level value=""WARN"" />
        //          </logger>
        //        </log4net>");

        //    ILoggerRepository rep = LogManager.CreateRepository(Guid.NewGuid().ToString());
        //    XmlConfigurator.Configure(rep, log4netConfig["log4net"]);

        //    //CategoryTraceListener categoryTraceListener = new CategoryTraceListener();
        //    //Debug.Listeners.Add(categoryTraceListener);

        //    log = LogManager.GetLogger(rep.Name);
        //    log.Info("OPEN");
        //}
        static void InitLog()
        {
            ILoggerRepository rep = LogManager.CreateRepository(Guid.NewGuid().ToString());

            ////ConsoleAppender
            FileAppender fa = new FileAppender(new SimpleLayout(), "RoslynProj.log");
            fa.ActivateOptions();
          //  fa.ErrorHandler = new SilentErrorHandler(); 
            //DebugAppender fa = new DebugAppender();
            //fa.Layout = new SimpleLayout();
            //fa.ActivateOptions();
            //fa.Category = null;

            //TestErrorHandler testErrHandler = new TestErrorHandler();
            //debugAppender.ErrorHandler = testErrHandler;
            //ILog log = LogManager.GetLogger(rep.Name, GetType());
            BasicConfigurator.Configure(rep, fa);
            log = LogManager.GetLogger(rep.Name, "RoslynProj");
            log.Info("InitLog");

            //String filename = "test.log";
            //SilentErrorHandler sh = new SilentErrorHandler();
            //ILogger log = CreateLogger(filename, new FileAppender.ExclusiveLock(), sh);
            //log.Log(null, Level.Info, "This is a message", null);
            //log.Log(GetType(), Level.Info, "This is a message 2", null);

        }

        /// <summary>
        /// Creates a logger hierarchy, configures a rolling file appender and returns an ILogger
        /// </summary>
        /// <param name="filename">The filename to log to</param>
        /// <param name="lockModel">The locking model to use.</param>
        /// <param name="handler">The error handler to use.</param>
        /// <returns>A configured ILogger</returns>
        //private static ILogger CreateLogger(string filename, FileAppender.LockingModelBase lockModel, IErrorHandler handler)
        //{
        //    return CreateLogger(filename, lockModel, handler, 100000, 0);
        //}

        /// <summary>
        /// Creates a logger hierarchy, configures a rolling file appender and returns an ILogger
        /// </summary>
        /// <param name="filename">The filename to log to</param>
        /// <param name="lockModel">The locking model to use.</param>
        /// <param name="handler">The error handler to use.</param>
        /// <param name="maxFileSize">Maximum file size for roll</param>
        /// <param name="maxSizeRollBackups">Maximum number of roll backups</param>
        /// <returns>A configured ILogger</returns>
        //private static ILogger CreateLogger(string filename, FileAppender.LockingModelBase lockModel, IErrorHandler handler, int maxFileSize, int maxSizeRollBackups)
        //{
        //    log4net.Repository.Hierarchy.Hierarchy h = (log4net.Repository.Hierarchy.Hierarchy)LogManager.CreateRepository("TestRepository");

        //    RollingFileAppender appender = new RollingFileAppender();
        //    appender.File = filename;
        //    appender.AppendToFile = false;
        //    appender.CountDirection = 0;
        //    appender.RollingStyle = RollingFileAppender.RollingMode.Size;
        //    appender.MaxFileSize = maxFileSize;
        //    appender.Encoding = Encoding.ASCII;
        //    appender.ErrorHandler = handler;
        //    appender.MaxSizeRollBackups = maxSizeRollBackups;
        //    if (lockModel != null)
        //    {
        //        appender.LockingModel = lockModel;
        //    }

        //    PatternLayout layout = new PatternLayout();
        //    layout.ConversionPattern = "%m%n";
        //    layout.ActivateOptions();

        //    appender.Layout = layout;
        //    appender.ActivateOptions();

        //    h.Root.AddAppender(appender);
        //    h.Configured = true;

        //    ILogger log = h.GetLogger("Logger");
        //    return log;
        //}


        static CCLog()
        {
            InitLog();
        }

        static ILog log = null;// LogManager.GetLogger("Roslyn");
        public static void Debug(object message)
        {
            Console.WriteLine(message);
            log.Debug(message);
        }
        public static void Error(object message)
        {
            Console.WriteLine(message);
            log.Error(message);

        }
        public static void Fatal(object message)
        {
            Console.WriteLine(message);
            log.Fatal(message);

        }
        public static void Info(object message)
        {
            Console.WriteLine(message);
            //log.Info(message);
        }
    }
}
