using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Xunit;
using Garnet.common;

namespace Garnet.Tests
{
    public class FormatTests
    {
        private class DummyLogger : ILogger
        {
            public string LastLogMessage { get; private set; }
            public LogLevel? LastLogLevel { get; private set; }
            public object[] LastState { get; private set; }

            public IDisposable BeginScope<TState>(TState state) => null;
            public bool IsEnabled(LogLevel logLevel) => true;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                LastLogLevel = logLevel;
                LastLogMessage = formatter(state, exception);
                LastState = new object[] { state, exception };
            }
        }

        [Fact]
        public void TryCreateEndpoint_WithNullOrEmpty_ReturnsDefaultBindAny()
        {
            var result = Format.TryCreateEndpoint(null, 1234);
            Assert.NotNull(result);
            result = Format.TryCreateEndpoint("", 1234);
            Assert.NotNull(result);
            result = Format.TryCreateEndpoint("   ", 1234);
            Assert.NotNull(result);
        }

        [Fact]
        public void TryCreateEndpoint_WithLocalhost_ReturnsLoopBack()
        {
            var result = Format.TryCreateEndpoint("localhost", 1234);
            Assert.Contains(result, ep => ((IPEndPoint)ep).Address.Equals(IPAddress.Loopback));
        }

        [Fact]
        public void TryCreateEndpoint_WithIpAddress_ReturnsSingleEndpoint()
        {
            var ip = "127.0.0.1";
            var result = Format.TryCreateEndpoint(ip, 1234);
            Assert.Single(result);
            Assert.Equal(IPAddress.Parse(ip), ((IPEndPoint)result[0]).Address);
        }

        [Fact]
        public void TryCreateEndpoint_WithHostnameAndNoConnect_LogsErrorAndReturnsNull()
        {
            var logger = new DummyLogger();
            var hostname = "nonexistenthostname";
            var result = Format.TryCreateEndpoint(hostname, 1234, tryConnect: false, logger: logger);
            Assert.Null(result);
            Assert.Contains("Provided hostname does not much acquired machine name", logger.LastLogMessage);
        }

        [Fact]
        public void TryCreateEndpoint_WithHostnameAndConnect_ReturnsEndpointIfListening()
        {
            var logger = new DummyLogger();
            // Use a hostname that resolves to localhost for test
            var hostname = "localhost";
            var result = Format.TryCreateEndpoint(hostname, 80, tryConnect: true, logger: logger);
            Assert.NotNull(result);
            Assert.All(result, ep => Assert.IsType<IPEndPoint>(ep));
        }

        [Fact]
        public void TryCreateEndpoint_LogsErrorOnDnsException()
        {
            var logger = new DummyLogger();
            // Use an invalid hostname to trigger exception
            var result = Format.TryCreateEndpoint("invalidhost", 1234, tryConnect: false, logger: logger);
            Assert.Null(result);
            Assert.Contains("Error while trying to resolve hostname", logger.LastLogMessage);
        }
    }
}
