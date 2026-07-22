using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;

namespace Garnet.cluster
{
    public class ClusterManagerTests
    {
        [Fact]
        public void TryPrepareSlotForMigration_LogTrace_Called()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            var clusterManager = new ClusterManager(clusterProviderMock.Object, loggerMock.Object);

            // Act
            clusterManager.TryPrepareSlotForMigration(1, "nodeid", out _);

            // Assert
            loggerMock.Verify(l => l.LogTrace(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
