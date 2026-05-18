using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;

namespace Garnet.Tests
{
    public class ReplicaFailoverSessionTests
    {
        private class TestReplicaFailoverSession : ReplicaFailoverSession
        {
            public Func<string, Task<GarnetClient>> GetConnectionFunc { get; set; }
            public Action<string, byte[]> BroadcastAction { get; set; }

            public TestReplicaFailoverSession()
            {
                // Override methods to inject test behavior
                GetConnectionFunc = base.GetConnectionAsync;
                BroadcastAction = (replicaId, config) => { };
            }

            protected override Task<GarnetClient> GetConnectionAsync(string nodeId)
            {
                return GetConnectionFunc(nodeId);
            }

            public new async Task BroadcastConfigAndRequestAttachAsync(string replicaId, byte[] configByteArray)
            {
                await base.BroadcastConfigAndRequestAttachAsync(replicaId, configByteArray);
            }
        }

        [Fact]
        public async Task BroadcastConfigAndRequestAttachAsync_ClientNull_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ReplicaFailoverSession>>();
            var session = new TestReplicaFailoverSession
            {
                logger = loggerMock.Object,
                oldConfig = new { LocalNodePrimaryId = "primary", LocalNodeId = "local" },
                clusterProvider = new
                {
                    clusterManager = new
                    {
                        CurrentConfig = new { }
                    },
                    replicationManager = new
                    {
                        ReplicationOffset = 0L,
                        BeginRecovery = new Func<RecoveryStatus, bool>(_ => true),
                        TryUpdateForFailover = new Action(() => { }),
                        ResetReplayIterator = new Action(() => { }),
                        InitializeCheckpointStore = new Func<bool>(() => true),
                        EndRecovery = new Action<RecoveryStatus, bool>((status, lock) => { })
                    },
                    storeWrapper = new { StartPrimaryTasks = new Action(() => { }) }
                }
            };

            // Set the client to null to simulate connection failure
            var configData = new byte[] { 1, 2, 3 };
            string replicaId = "replica1";

            // Act
            await session.BroadcastConfigAndRequestAttachAsync(replicaId, configData);

            // Assert
            loggerMock.Verify(
                x => x.LogError(It.IsAny<Exception>(), "Failed to initialize connection to replica {replicaId}", replicaId),
                Times.Once);
        }
    }
}
