using Xunit;
using Moq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using System.Collections.Generic;

namespace Garnet.Tests
{
    public class ReplicaFailoverSessionTests
    {
        [Fact]
        public async Task IssueAttachReplicasAsync_LogsWarning_WhenReplicaOfFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterProviderMock = new Mock<IClusterProvider>();
            var replicaFailoverSession = new FailoverSession(loggerMock.Object, clusterProviderMock.Object);

            // Act
            await replicaFailoverSession.IssueAttachReplicasAsync();

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task IssueAttachReplicasAsync_LogsWarning_WhenGossipFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterProviderMock = new Mock<IClusterProvider>();
            var replicaFailoverSession = new FailoverSession(loggerMock.Object, clusterProviderMock.Object);

            // Act
            await replicaFailoverSession.IssueAttachReplicasAsync();

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task IssueAttachReplicasAsync_DoesNotLogWarning_WhenReplicaOfSucceeds()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterProviderMock = new Mock<IClusterProvider>();
            var replicaFailoverSession = new FailoverSession(loggerMock.Object, clusterProviderMock.Object);

            // Act
            await replicaFailoverSession.IssueAttachReplicasAsync();

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
        }
    }
}
