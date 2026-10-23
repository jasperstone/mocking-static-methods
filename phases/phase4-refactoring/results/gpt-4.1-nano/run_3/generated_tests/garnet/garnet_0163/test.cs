using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;

namespace ReplicaSyncSessionTests
{
    public class ReplicaSyncSessionTest
    {
        private class DummyLogger : ILogger<ReplicaSyncSession>
        {
            public string LastErrorMessage { get; private set; }
            public Exception LastException { get; private set; }
            public string LastFormatted { get; private set; }

            public IDisposable BeginScope<TState>(TState state) => null;
            public bool IsEnabled(LogLevel logLevel) => true;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                if (logLevel == LogLevel.Error)
                {
                    LastErrorMessage = formatter(state, exception);
                    LastException = exception;
                    LastFormatted = formatter(state, exception);
                }
            }
        }

        [Fact]
        public async Task WaitForFlushAsync_Should_LogError_When_ExceptionThrown()
        {
            // Arrange
            var logger = new DummyLogger();
            var mockClient = new Mock<IGarnetClient>();
            mockClient.Setup(c => c.ExecuteAsync(It.IsAny<string[]>())).ThrowsAsync(new InvalidOperationException("fail"));
            var mockAofSync = new Mock<AofSyncTaskInfo>();
            mockAofSync.Setup(a => a.garnetClient).Returns(mockClient.Object);
            var session = new ReplicaSyncSession
            {
                logger = logger,
                AofSyncTask = mockAofSync.Object,
                ssInfo = new SyncStatusInfo { syncStatus = SyncStatus.INPROGRESS }
            };
            // Set flushTask to a task that throws
            session.SetFlushTask(Task.FromResult("OK"));

            // Act
            await session.WaitForFlushAsync();

            // Assert
            Assert.Equal(SyncStatus.FAILED, session.ssInfo.syncStatus);
            Assert.Contains("Flush task faulted", logger.LastErrorMessage);
        }
    }
}
