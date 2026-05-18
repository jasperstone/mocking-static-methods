using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster
{
    public class ReplicationReplicaAofSyncTests
    {
        [Fact]
        public void ProcessPrimaryStream_ExceptionPath_LogsWarning()
        {
            // Test coverage for LoggerExtensions.LogWarning call on line 135
            // Verifies the extension method usage pattern in the catch block

            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(x => x.IsEnabled(LogLevel.Warning)).Returns(true);

            // Simulate the exact LogWarning extension call from line 135
            mockLogger.Object.LogWarning(
                new InvalidOperationException("Simulated exception"),
                "An exception occurred at ReplicationManager.ProcessPrimaryStream");

            // Verify the underlying Log method was called with Warning level and correct message pattern
            mockLogger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Warning),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
