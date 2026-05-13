using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;

namespace Garnet.Tests
{
    public class ReplicaSyncSessionTests
    {
        private class DummyLogger : ILogger
        {
            public string LastLogMessage { get; private set; }
            public LogLevel LastLogLevel { get; private set; }
            public Exception LastException { get; private set; }

            public IDisposable BeginScope<TState>(TState state) => null;
            public bool IsEnabled(LogLevel logLevel) => true;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                LastLogMessage = formatter(state, exception);
                LastLogLevel = logLevel;
                LastException = exception;
            }
        }

        [Fact]
        public async Task WaitForFlushAsync_CatchesException_LogsErrorAndSetsFailed()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var session = new ReplicaSyncSession
            {
                logger = mockLogger.Object,
                ssInfo = new SyncStatusInfo { syncStatus = SyncStatus.INPROGRESS },
                flushTask = Task.FromException(new InvalidOperationException("test exception"))
            };

            // Act
            await session.WaitForFlushAsync();

            // Assert
            mockLogger.Verify(l => l.LogError(It.IsAny<Exception>(), "{method}", "WaitForFlushAsync"), Times.Once);
            Assert.Equal(SyncStatus.FAILED, session.ssInfo.syncStatus);
        }
    }
}
