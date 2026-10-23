using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Tests.cluster
{
    public class AofSyncTaskInfoLoggerTests
    {
        [Fact]
        public void LoggerExtensions_LogInformation_CalledWithExpectedMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var remoteNodeId = "remoteNodeId";
            var startAddress = 123L;

            loggerMock.Setup(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Starting ReplicationManager.ReplicaSyncTask")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()))
                .Verifiable();

            // Act
            loggerMock.Object.LogInformation("Starting ReplicationManager.ReplicaSyncTask for remote node {remoteNodeId} starting from address {address}", remoteNodeId, startAddress);

            // Assert
            loggerMock.Verify();
        }
    }
}
