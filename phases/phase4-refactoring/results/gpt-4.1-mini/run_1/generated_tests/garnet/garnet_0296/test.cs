using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Xunit;
using Garnet.common;

namespace Garnet.Tests
{
    public class FormatTests
    {
        private class TestLogger : ILogger
        {
            public string LastLogMessage { get; private set; }
            public LogLevel? LastLogLevel { get; private set; }
            public EventId? LastEventId { get; private set; }
            public Exception LastException { get; private set; }
            public object[] LastArgs { get; private set; }

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
                    var args = new object[kvps.Count - 1];
                    for (int i = 0; i < args.Length; i++)
                    {
                        args[i] = kvps[i].Value;
                    }
                    LastArgs = args;
                }
            }
        }

        [Fact]
        public void TryCreateEndpoint_LogsErrorWhenNoIpAddressFound()
        {
            var logger = new TestLogger();
            var result = Format.TryCreateEndpoint("nonexistent.hostname.example", 1234, tryConnect: false, logger);

            Assert.Null(result);
            Assert.NotNull(logger.LastLogMessage);
            Assert.Contains("No IP address found for hostname", logger.LastLogMessage);
            Assert.Equal(LogLevel.Error, logger.LastLogLevel);
        }

        [Fact]
        public void TryCreateEndpoint_LogsErrorWhenHostnameDoesNotMatchMachineName()
        {
            var logger = new TestLogger();

            // We use a hostname that is unlikely to match the machine hostname
            var result = Format.TryCreateEndpoint("unlikelyhostname12345", 1234, tryConnect: false, logger);

            Assert.Null(result);
            Assert.NotNull(logger.LastLogMessage);
            Assert.Contains("Provided hostname does not much acquired machine name", logger.LastLogMessage);
            Assert.Equal(LogLevel.Error, logger.LastLogLevel);
        }

        [Fact]
        public void TryCreateEndpoint_LogsErrorWhenNoReachableIpAddressFound()
        {
            var logger = new TestLogger();

            // Use a hostname that resolves to IPs but tryConnect true so it tries to connect and fails
            // We use localhost but with tryConnect true to force connection attempts
            var result = Format.TryCreateEndpoint("localhost", 65000, tryConnect: true, logger);

            // It should return null because no reachable IP address found on port 65000 (likely closed)
            Assert.Null(result);
            Assert.NotNull(logger.LastLogMessage);
            Assert.Contains("No reachable IP address found for hostname", logger.LastLogMessage);
            Assert.Equal(LogLevel.Error, logger.LastLogLevel);
        }
    }
}
