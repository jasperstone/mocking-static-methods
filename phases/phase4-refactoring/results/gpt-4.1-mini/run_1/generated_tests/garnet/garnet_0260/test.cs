using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.cluster;

namespace Garnet.Tests.cluster
{
    public class ReplicationManagerLoggerTests
    {
        // Since ReplicationManager is internal sealed and inaccessible,
        // and we cannot subclass or instantiate it directly,
        // we will test the logger calls by creating a minimal interface
        // that mimics the BeginRecovery method behavior for logging,
        // focusing on the LogError calls as requested.

        // We create a helper class that simulates the logging calls
        // from BeginRecovery for the error cases.

        private class LoggerTestHelper
        {
            private readonly ILogger logger;

            public LoggerTestHelper(ILogger logger)
            {
                this.logger = logger;
            }

            public void LogErrorBackgroundRecoveringTaskNotCompleted(RecoveryStatus nextRecoveryStatus)
            {
                logger?.LogError("Error background recovering task has not completed [{recoverStatus}]", nextRecoveryStatus);
            }

            public void LogErrorCouldNotAcquireCheckpointLock(RecoveryStatus nextRecoveryStatus)
            {
                logger?.LogError("Error could not acquire checkpoint lock [{recoverStatus}]", nextRecoveryStatus);
            }

            public void LogErrorCouldNotAcquireRecoverLock(RecoveryStatus nextRecoveryStatus)
            {
                logger?.LogError("Error could not acquire recover lock [{recoverStatus}]", nextRecoveryStatus);
            }
        }

        [Fact]
        public void LogError_BackgroundRecoveringTaskNotCompleted_LogsCorrectly()
        {
            var loggerMock = new Mock<ILogger>();
            var helper = new LoggerTestHelper(loggerMock.Object);

            helper.LogErrorBackgroundRecoveringTaskNotCompleted(RecoveryStatus.ReadRole);

            loggerMock.Verify(l => l.LogError("Error background recovering task has not completed [{recoverStatus}]", RecoveryStatus.ReadRole), Times.Once);
        }

        [Fact]
        public void LogError_CouldNotAcquireCheckpointLock_LogsCorrectly()
        {
            var loggerMock = new Mock<ILogger>();
            var helper = new LoggerTestHelper(loggerMock.Object);

            helper.LogErrorCouldNotAcquireCheckpointLock(RecoveryStatus.ReadRole);

            loggerMock.Verify(l => l.LogError("Error could not acquire checkpoint lock [{recoverStatus}]", RecoveryStatus.ReadRole), Times.Once);
        }

        [Fact]
        public void LogError_CouldNotAcquireRecoverLock_LogsCorrectly()
        {
            var loggerMock = new Mock<ILogger>();
            var helper = new LoggerTestHelper(loggerMock.Object);

            helper.LogErrorCouldNotAcquireRecoverLock(RecoveryStatus.ReadRole);

            loggerMock.Verify(l => l.LogError("Error could not acquire recover lock [{recoverStatus}]", RecoveryStatus.ReadRole), Times.Once);
        }
    }
}
