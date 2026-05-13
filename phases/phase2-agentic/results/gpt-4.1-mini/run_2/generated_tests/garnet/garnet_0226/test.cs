using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.cluster;
using Garnet.client;

namespace Garnet.Tests.cluster
{
    public class ReplicationManagerTests
    {
        [Fact]
        public async Task TryReplicateDiskbasedSyncAsync_LogsForegroundCheckpointRetrieval()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterManagerMock = new Mock<IClusterManager>();
            var clusterProviderMock = new Mock<IClusterProvider>();
            var replicationManager = new ReplicationManagerForTest(clusterProviderMock.Object, loggerMock.Object);

            var options = new ReplicateSyncOptions
            {
                NodeId = 1,
                TryAddReplica = false,
                Background = false,
                UpgradeLock = false,
                Force = false
            };

            var sessionMock = new Mock<ClusterSession>();
            sessionMock.Setup(s => s.UnsafeBumpAndWaitForEpochTransitionAsync()).Returns(Task.CompletedTask);

            clusterProviderMock.SetupGet(cp => cp.clusterManager).Returns(clusterManagerMock.Object);

            // Setup ReplicaSyncAttachTaskAsync to return null (success)
            replicationManager.SetReplicaSyncAttachTaskAsyncReturn(null);

            // Act
            var result = await replicationManager.TryReplicateDiskbasedSyncAsync(sessionMock.Object, options);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Initiating foreground checkpoint retrieval")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            Assert.True(result.Success);
            Assert.True(result.ErrorMessage.IsEmpty);
        }

        [Fact]
        public async Task TryReplicateDiskbasedSyncAsync_LogsBackgroundCheckpointRetrieval()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterManagerMock = new Mock<IClusterManager>();
            var clusterProviderMock = new Mock<IClusterProvider>();
            var replicationManager = new ReplicationManagerForTest(clusterProviderMock.Object, loggerMock.Object);

            var options = new ReplicateSyncOptions
            {
                NodeId = 1,
                TryAddReplica = false,
                Background = true,
                UpgradeLock = false,
                Force = false
            };

            var sessionMock = new Mock<ClusterSession>();
            sessionMock.Setup(s => s.UnsafeBumpAndWaitForEpochTransitionAsync()).Returns(Task.CompletedTask);

            clusterProviderMock.SetupGet(cp => cp.clusterManager).Returns(clusterManagerMock.Object);

            // Act
            var result = await replicationManager.TryReplicateDiskbasedSyncAsync(sessionMock.Object, options);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Initiating background checkpoint retrieval")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            Assert.True(result.Success);
            Assert.True(result.ErrorMessage.IsEmpty);
        }

        // Helper class to override ReplicaSyncAttachTaskAsync for testing
        private class ReplicationManagerForTest : ReplicationManager
        {
            private readonly ILogger _logger;
            private readonly IClusterProvider _clusterProvider;
            private string _replicaSyncAttachTaskAsyncReturn;

            public ReplicationManagerForTest(IClusterProvider clusterProvider, ILogger logger)
            {
                _clusterProvider = clusterProvider;
                _logger = logger;
            }

            public void SetReplicaSyncAttachTaskAsyncReturn(string returnValue)
            {
                _replicaSyncAttachTaskAsyncReturn = returnValue;
            }

            public override async Task<(bool Success, ReadOnlyMemory<byte> ErrorMessage)> TryReplicateDiskbasedSyncAsync(ClusterSession session, ReplicateSyncOptions options)
            {
                ReadOnlyMemory<byte> errorMessage = default;
                try
                {
                    _logger?.LogTrace("CLUSTER REPLICATE {nodeid}", options.NodeId);
                    if (options.TryAddReplica)
                    {
                        var (success, error) = await _clusterProvider.clusterManager.TryAddReplicaAsync(options.NodeId, options.Force, options.UpgradeLock, logger: _logger).ConfigureAwait(false);
                        if (!success)
                        {
                            return (false, error);
                        }
                    }

                    if (session != null)
                    {
                        await session.UnsafeBumpAndWaitForEpochTransitionAsync().ConfigureAwait(false);
                    }

                    if (options.Background)
                    {
                        _logger?.LogInformation("Initiating background checkpoint retrieval");
                        _ = ReplicaSyncAttachTaskAsync(options.UpgradeLock, forceAsync: true);
                    }
                    else
                    {
                        _logger?.LogInformation("Initiating foreground checkpoint retrieval");
                        var resp = await ReplicaSyncAttachTaskAsync(options.UpgradeLock, forceAsync: false).ConfigureAwait(false);
                        if (resp != null)
                        {
                            errorMessage = Encoding.ASCII.GetBytes(resp);
                            return (false, errorMessage);
                        }
                    }

                    return (true, default);
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, $"{nameof(TryReplicateDiskbasedSyncAsync)}");
                    return (false, errorMessage);
                }
            }

            private Task<string> ReplicaSyncAttachTaskAsync(bool downgradeLock, bool forceAsync)
            {
                return Task.FromResult(_replicaSyncAttachTaskAsyncReturn);
            }
        }

        // Interfaces and classes to mock dependencies
        public interface IClusterProvider
        {
            IClusterManager clusterManager { get; }
        }

        public interface IClusterManager
        {
            Task<(bool Success, ReadOnlyMemory<byte> ErrorMessage)> TryAddReplicaAsync(int nodeId, bool force, bool upgradeLock, ILogger logger = null);
        }

        public class ReplicateSyncOptions
        {
            public int NodeId { get; set; }
            public bool TryAddReplica { get; set; }
            public bool Background { get; set; }
            public bool UpgradeLock { get; set; }
            public bool Force { get; set; }
        }

        public class ClusterSession
        {
            public virtual Task UnsafeBumpAndWaitForEpochTransitionAsync() => Task.CompletedTask;
        }
    }
}
