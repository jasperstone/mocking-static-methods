using System;
using System.Net;
using Microsoft.Extensions.Logging;
using Xunit;
using Garnet.common;

namespace Garnet.Tests
{
    public class FormatTests
    {
        private class TestLogger : ILogger
        {
            public string LastLogMessage;
            public LogLevel LastLogLevel;
            public EventId LastEventId;
            public Exception LastException;
            public object[] LastArgs;

            public IDisposable BeginScope<TState>(TState state) => null;
            public bool IsEnabled(LogLevel logLevel) => true;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                LastLogLevel = logLevel;
                LastEventId = eventId;
                LastException = exception;
                LastLogMessage = formatter(state, exception);
                if (state is IReadOnlyList<KeyValuePair<string, object>> kvps)
                {
                    LastArgs = new object[kvps.Count];
                    for (int i = 0; i < kvps.Count; i++)
                        LastArgs[i] = kvps[i].Value;
                }
            }
        }

        [Fact]
        public void TryCreateEndpoint_LogsErrorWhenNoIpAddressFound()
        {
            var logger = new TestLogger();

            // Use a hostname that resolves to no IP addresses by passing an unlikely hostname
            var result = Format.TryCreateEndpoint("no-such-hostname-should-not-exist-12345", 1234, logger: logger);

            Assert.Null(result);
            Assert.Equal(LogLevel.Error, logger.LastLogLevel);
            Assert.Contains("No IP address found for hostname", logger.LastLogMessage);
            Assert.Contains("no-such-hostname-should-not-exist-12345", logger.LastLogMessage);
        }

        [Fact]
        public void TryCreateEndpoint_LogsErrorWhenProvidedHostnameDoesNotMatchMachineName()
        {
            var logger = new TestLogger();

            // Use a hostname that resolves to some IPs but does not match machine hostname
            var result = Format.TryCreateEndpoint("some-random-hostname-12345", 1234, tryConnect: false, logger: logger);

            Assert.Null(result);
            Assert.Equal(LogLevel.Error, logger.LastLogLevel);
            Assert.Contains("Provided hostname does not much acquired machine name", logger.LastLogMessage);
            Assert.Contains("some-random-hostname-12345", logger.LastLogMessage);
        }

        [Fact]
        public void TryCreateEndpoint_LogsErrorWhenNoReachableIpAddressFound()
        {
            var logger = new TestLogger();

            // Use a hostname that resolves to IPs but tryConnect is true and no IP is reachable
            var result = Format.TryCreateEndpoint("localhost", 65000, tryConnect: true, logger: logger);

            Assert.Null(result);
            Assert.Equal(LogLevel.Error, logger.LastLogLevel);
            Assert.Contains("No reachable IP address found for hostname", logger.LastLogMessage);
            Assert.Contains("localhost", logger.LastLogMessage);
        }
    }
}
