using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;

namespace Garnet.Tests
{
    public class FailoverSessionTests
    {
        [Fact]
        public async Task PauseWritesAndWaitForSyncAsync_ClientNull_LogsError()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<FailoverSession>>();
            var mockClusterProvider = new Mock<ClusterProvider>();
            var mockReplicationManager = new Mock<ReplicationManager>();
            var mockClusterManager = new Mock<ClusterManager>();
            var mockStoreWrapper = new Mock<StoreWrapper>();
            var session = new FailoverSession
            {
                logger = mockLogger.Object,
                clusterProvider = new ClusterProvider
                {
                    replicationManager = mockReplicationManager.Object,
                    clusterManager = mockClusterManager.Object,
                    storeWrapper = mockStoreWrapper.Object,
                },
                oldConfig = new Config
                {
                    LocalNodePrimaryId = "primaryId",
                    LocalNodeId = "localId"
                },
                cts = new CancellationTokenSource(),
                failoverTimeout = TimeSpan.FromSeconds(1),
                status = FailoverStatus.None
            };

            // Create a derived class to override GetConnectionAsync to return null
            var testSession = new TestFailoverSession
            {
                logger = mockLogger.Object,
                oldConfig = session.oldConfig,
                cts = session.cts,
                failoverTimeout = session.failoverTimeout,
                status = session.status,
                clusterProvider = session.clusterProvider
            };

            // Act
            var result = await testSession.PauseWritesAndWaitForSyncAsync();

            // Assert
            Assert.False(result);
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to initialize connection to primary")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }

        private class TestFailoverSession : FailoverSession
        {
            public override Task<GarnetClient> GetConnectionAsync(string nodeId)
            {
                // Return null to simulate failure
                return Task.FromResult<GarnetClient>(null);
            }
        }
    }
}
