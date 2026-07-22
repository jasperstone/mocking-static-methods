using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Garnet.cluster;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Tests
{
    public class ReplicationManagerTests
    {
        [Fact]
        public unsafe void ProcessPrimaryStream_ShouldLogWarning_WhenNodeIsNotReplica()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ReplicationManager>>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            var appendOnlyFileMock = new Mock<AppendOnlyFile>();
            var defaultDatabaseMock = new Mock<DefaultDatabase>();
            var clusterManagerMock = new Mock<ClusterManager>();
            var activeReplayMock = new Mock<IActiveReplay>();
            var replicationManager = new ReplicationManager();

            // Setup dependencies
            clusterProviderMock.Setup(cp => cp.clusterManager).Returns(clusterManagerMock.Object);
            clusterProviderMock.Setup(cp => cp.storeWrapper).Returns(storeWrapperMock.Object);
            clusterProviderMock.Setup(cp => cp.appendOnlyFile).Returns(appendOnlyFileMock.Object);
            clusterProviderMock.Setup(cp => cp.DefaultDatabase).Returns(defaultDatabaseMock.Object);
            // Set server options
            var serverOptions = new ServerOptions { EnableFastCommit = false, FastAofTruncate = false, ReplOffsetMaxLag = 0 };
            clusterProviderMock.Setup(cp => cp.serverOptions).Returns(serverOptions);
            // Set cluster config with role as non-replica
            var config = new ClusterConfig { LocalNodeRole = NodeRole.MASTER, LocalNodeId = 1 };
            var clusterManager = new ClusterManager { CurrentConfig = config };
            clusterProviderMock.Setup(cp => cp.clusterManager).Returns(clusterManager);
            // Set the clusterProvider in the ReplicationManager
            var repManager = new ReplicationManager
            {
                clusterProvider = clusterProviderMock.Object,
                logger = loggerMock.Object,
                activeReplay = activeReplayMock.Object,
                storeWrapper = storeWrapperMock.Object
            };

            // Prepare a record buffer
            byte[] recordData = { 1, 2, 3, 4, 5 };
            fixed (byte* recordPtr = recordData)
            {
                // Act
                var exceptionThrown = false;
                try
                {
                    repManager.ProcessPrimaryStream(recordPtr, recordData.Length, 0, 12345, 67890);
                }
                catch (GarnetException ex)
                {
                    exceptionThrown = true;
                    // Verify that LogWarning was called with the expected message
                    loggerMock.Verify(
                        log => log.Log(
                            LogLevel.Warning,
                            It.IsAny<EventId>(),
                            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"This node {config.LocalNodeId} is not a replica")),
                            null,
                            It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                        Times.Once);
                }
                Assert.True(exceptionThrown, "Expected GarnetException to be thrown");
            }
        }
    }
}
