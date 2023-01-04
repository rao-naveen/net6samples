using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MELConsoleLogIntegraton
{
    /// <summary>
    ///  Ref https://github.com/Cysharp/ZLogger
    /// </summary>
    // Own static logger manager

    public static class LogManager
    {
        static ILogger globalLogger;
        static ILoggerFactory loggerFactory;

        public static void SetLoggerFactory(ILoggerFactory loggerFactory, string categoryName)
        {
            LogManager.loggerFactory = loggerFactory;
            LogManager.globalLogger = loggerFactory.CreateLogger(categoryName);
        }

        public static ILogger Logger => globalLogger;

        public static ILogger<T> GetLogger<T>() where T : class => loggerFactory.CreateLogger<T>();
        public static ILogger GetLogger(string categoryName) => loggerFactory.CreateLogger(categoryName);
    }
    // // You can use this logger manager like following.
    public class Foo
    {
        public static readonly ILogger<Foo> logger = LogManager.GetLogger<Foo>();

        public Foo(int x)
        {
            logger.LogDebug("do do do: {0}", x);
        }
    }
}
