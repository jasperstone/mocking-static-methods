using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.cluster;

namespace Garnet.Tests
{
    public class ReplicaSyncSessionTests
    {
        [Fact]
        public async Task SendCheckpointAsync_LogsErrorWhenReplicaNodeIdUnknown()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();

            var mockStoreWrapper = new Mock<StoreWrapper>();
            var mockClusterProvider = new Mock<ClusterProvider>();

            // Setup clusterManager.CurrentConfig.GetWorkerAddressFromNodeId to return null address and -1 port
            var mockClusterManager = new Mock<IClusterManager>();
            var mockCurrentConfig = new Mock<IClusterConfig>();
            mockCurrentConfig.Setup(c => c.GetWorkerAddressFromNodeId(It.IsAny<string>())).Returns((null, -1));
            mockClusterManager.SetupGet(m => m.CurrentConfig).Returns(mockCurrentConfig.Object);
            mockClusterProvider.SetupGet(cp => cp.clusterManager).Returns(mockClusterManager.Object);

            var session = new ReplicaSyncSession(
                mockStoreWrapper.Object,
                mockClusterProvider.Object,
                replicaNodeId: "unknownNode",
                logger: mockLogger.Object);

            // Act
            var result = await session.SendCheckpointAsync();

            // Assert
            Assert.False(result);
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("PRIMARY-ERR don't know about replicaId")),
                    null,
                    It.IsAny<Func<It.IsAnyType, System.Exception, string>>()),
                Times.Once);
        }
    }

    // Interfaces to mock clusterManager and CurrentConfig
    public interface IClusterManager
    {
        IClusterConfig CurrentConfig { get; }
    }

    public interface IClusterConfig
    {
        (string, int) GetWorkerAddressFromNodeId(string nodeId);
    }
}
