using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;

namespace Garnet.Tests
{
    public class MigrateSessionTests
    {
        private class DummyLogger : ILogger
        {
            public List<(LogLevel level, string message, Exception ex)> Logs = new List<(LogLevel, string, Exception)>();
            public IDisposable BeginScope<TState>(TState state) => null;
            public bool IsEnabled(LogLevel logLevel) => true;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                var message = formatter(state, exception);
                Logs.Add((logLevel, message, exception));
            }
        }

        [Fact]
        public async Task LogError_CalledOnException()
        {
            // Arrange
            var logger = new DummyLogger();
            var mockClient = new Mock<IGarnetClient>();
            mockClient.Setup(c => c.ExecuteForArrayAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                      .ThrowsAsync(new InvalidOperationException("Test exception"));

            var migrateOperation = new Mock<IMigrateOperation>();
            migrateOperation.Setup(m => m.Client).Returns(mockClient.Object);

            var migrateOperations = new[] { migrateOperation.Object };
            var migrateSession = new MigrateSession
            {
                Logger = logger,
                MigrateOperation = migrateOperations,
                _targetNodeId = 1,
                _sourceNodeId = 1,
                _replaceOption = false,
                _namespaces = new List<ulong> { 0, 1, 2, 3 },
                _namespaceMap = null
            };

            // Act
            var result = await migrateSession.ReserveDestinationVectorSetsAsync();

            // Assert
            Assert.False(result);
            var errorLogs = logger.Logs.Where(l => l.level == LogLevel.Error).ToList();
            Assert.NotEmpty(errorLogs);
            Assert.Contains(errorLogs, l => l.message.Contains("Failed to reserve"));
        }
    }
}
