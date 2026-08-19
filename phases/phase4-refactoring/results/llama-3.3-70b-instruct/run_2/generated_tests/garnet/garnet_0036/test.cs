using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

namespace Garnet.cluster
{
    public class ClusterManagerTests
    {
        [Fact]
        public void TryPrepareSlotForMigration_LogTraceCalled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterProvider = new ClusterProvider(new StoreWrapper());
            var clusterManager = new ClusterManager(clusterProvider, loggerMock.Object);

            // Act
            clusterManager.TryPrepareSlotForMigration(1, "nodeid", out _);

            // Assert
            loggerMock.Verify(l => l.LogTrace(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
