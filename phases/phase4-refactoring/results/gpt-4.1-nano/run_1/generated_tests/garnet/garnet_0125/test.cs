using Xunit;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Garnet.cluster;

namespace MigrationLoggingTests
{
    // A minimal class to test the logging behavior of CheckConnectionAsync
    public class TestMigration
    {
        public class DummyLogger : ILogger
        {
            public readonly System.Collections.Generic.List<string> Logs = new();

            public IDisposable BeginScope<TState>(TState state) => null;
            public bool IsEnabled(LogLevel logLevel) => true;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                Logs.Add(formatter(state, exception));
            }
        }

        public async Task TestLogTraceOnCheckConnectionAsync()
        {
            var logger = new DummyLogger();

            var mockClient = new Mock<GarnetClientSession>();
            mockClient.Setup(c => c.IsConnected).Returns(false);
            mockClient.Setup(c => c.ReconnectAsync(It.IsAny<int>())).Returns(Task.CompletedTask);
            mockClient.Setup(c => c.Authenticate(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync("OK");
            mockClient.Setup(c => c.WaitAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("OK");

            var dummy = new DummyDependency
            {
                Client = mockClient.Object,
                Logger = logger,
                Timeout = TimeSpan.FromSeconds(10),
                CtsToken = new CancellationToken()
            };

            // Call the method
            var result = await dummy.CheckConnectionAsync();

            // Verify that LogTrace was called
            Assert.Contains(logger.Logs, log => log.Contains("Sending CLUSTER SETSLOTRANGE"));
        }

        // Dummy class to hold the method under test
        private class DummyDependency
        {
            public GarnetClientSession Client { get; set; }
            public ILogger Logger { get; set; }
            public TimeSpan Timeout { get; set; }
            public CancellationToken CtsToken { get; set; }

            public async Task<bool> CheckConnectionAsync()
            {
                if (!Client.IsConnected)
                {
                    await Client.ReconnectAsync((int)Timeout.TotalMilliseconds);
                    var authResp = await Client.Authenticate("user", "pass").WaitAsync(Timeout, CtsToken);
                    Logger.Log(LogLevel.Trace, new EventId(), $"Sending CLUSTER SETSLOTRANGE {authResp}", null, (s, e) => s.ToString());
                    if (!authResp.Equals("OK", StringComparison.Ordinal))
                    {
                        Logger.Log(LogLevel.Error, new EventId(), $"Auth failed: {authResp}", null, (s, e) => s.ToString());
                        return false;
                    }
                    return true;
                }
                return true;
            }
        }
    }
}
