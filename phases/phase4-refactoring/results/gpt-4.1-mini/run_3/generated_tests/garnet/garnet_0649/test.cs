using System;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Tsavorite.Tests
{
    public class LoggerExtensionsTests
    {
        private class TestLogger : ILogger
        {
            public readonly System.Collections.Generic.List<string> LoggedMessages = new();

            public IDisposable BeginScope<TState>(TState state) => null!;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                if (logLevel == LogLevel.Information)
                {
                    LoggedMessages.Add(formatter(state, exception));
                }
            }
        }

        [Fact]
        public void LogInformation_Extension_LogsExpectedMessage()
        {
            var logger = new TestLogger();
            ILogger ilogger = logger;

            ilogger.LogInformation("Recovery called on non-empty log - resetting to empty state first. Make sure store is quiesced before calling Recover on a running store.");

            Assert.Contains("Recovery called on non-empty log", logger.LoggedMessages);
        }
    }
}
