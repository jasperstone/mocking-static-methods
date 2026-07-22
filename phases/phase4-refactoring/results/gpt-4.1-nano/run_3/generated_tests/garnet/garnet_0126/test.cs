using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using Garnet.cluster;

namespace Garnet.Tests
{
    public class MigrationDriverTests
    {
        [Fact]
        public async Task TrySetSlotRangesAsync_ShouldLogError_WhenResultIsNotOk()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockClient = new Mock<GarnetClientSession>();
            var mockClusterProvider = new Mock<ClusterProvider>();
            var mockClusterManager = new Mock<ClusterManager>();
            var mockStoreWrapper = new Mock<StoreWrapper>();
            var mockMigrationManager = new Mock<MigrationManager>();

            // Setup the client to simulate a non-"OK" result
            mockClient.Setup(c => c.SetSlotRange(It.IsAny<Memory<byte>>(), It.IsAny<string>(), It.IsAny<List<(int, int)>>()))
                      .ReturnsAsync("ERROR");

            // Setup the cluster provider to return the mocked client
            mockClusterProvider.Setup(cp => cp.storeWrapper).Returns(mockStoreWrapper.Object);
            mockClusterProvider.Setup(cp => cp.migrationManager).Returns(mockMigrationManager.Object);
            mockClusterProvider.Setup(cp => cp.clusterManager).Returns(mockClusterManager.Object);
            mockClusterProvider.Setup(cp => cp.loggerFactory).Returns(new LoggerFactory());

            // Instantiate the MigrationSession with minimal setup
            var session = new MigrateSession(
                clusterSession: null,
                clusterProvider: mockClusterProvider.Object,
                _targetAddress: "127.0.0.1",
                _targetPort: 6379,
                _targetNodeId: "nodeid",
                _username: null,
                _passwd: null,
                _sourceNodeId: "sourceid",
                _copyOption: false,
                _replaceOption: false,
                _timeout: 1000,
                _slots: new System.Collections.Generic.HashSet<int> { 1, 2, 3 },
                sketch: null,
                transferOption: TransferOption.SLOTS);

            // Inject the logger
            typeof(MigrateSession).GetProperty("logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(session, mockLogger.Object);

            // Act
            var result = await session.TrySetSlotRangesAsync("nodeid", MigrateState.IMPORT);

            // Assert
            Assert.False(result);
            mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("SetSlotRange error: ERROR")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
