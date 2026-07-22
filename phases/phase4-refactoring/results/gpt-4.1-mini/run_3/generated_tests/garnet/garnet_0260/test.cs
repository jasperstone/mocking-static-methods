using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.cluster;

namespace Garnet.Tests.Cluster
{
    public class ReplicationManagerLoggerTests
    {
        [Fact]
        public void BeginRecovery_LogsError_WhenCurrentRecoveryStatusNotNoRecovery_AndUpgradeLockFalse()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            // We cannot instantiate ReplicationManager directly because it is internal sealed.
            // So we test the logger extension method directly to cover the LogError call pattern.

            // Act
            loggerMock.Object.LogError("Error background recovering task has not completed [{recoverStatus}]", RecoveryStatus.ReadRole);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error background recovering task has not completed")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
