using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace JabbR_Core.Data.Logging
{
    public class FileLoggerProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName)
        {
            return new FileLogger();
        }

        public void Dispose()
        { }

        private class FileLogger : ILogger
        {
            private static string lockObject = "";
            public bool IsEnabled(LogLevel logLevel)
            {
                return true;
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                //Log to prevent issues with file currently being accessed with overlapping requests
                //This slows things down but ensures proper serialization in text file.
                lock (lockObject)
                {
                    //File.AppendAllText(@"entityframework.log", formatter(state, exception));
                    Console.WriteLine(formatter(state, exception));
                }
            }

            public IDisposable BeginScope<TState>(TState state)
            {
                return null;
            }
        }
    }
}
