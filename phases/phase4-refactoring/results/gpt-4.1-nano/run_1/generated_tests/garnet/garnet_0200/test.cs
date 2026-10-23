using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using Garnet.server;
using Garnet.common;

namespace Garnet.Tests
{
    public class ReplicaSyncSessionLogTests
    {
        [Fact]
        public async Task LogInformation_IsCalledOnLine463()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockClusterProvider = new Mock<ClusterProvider>();
            var mockCheckpointEntry = new CheckpointEntry
            {
                metadata = new CheckpointMetadata
                {
                    storeVersion = 1,
                    objectStoreVersion = 1,
                    storePrimaryReplId = "id",
                    objectStorePrimaryReplId = "id"
                }
            };

            // Create a minimal instance of ReplicaSyncSession with the logger
            var session = new ReplicaSyncSession(
                storeWrapper: null,
                clusterProvider: mockClusterProvider.Object,
                replicaSyncMetadata: null,
                token: CancellationToken.None,
                replicaNodeId: "node1",
                replicaAssignedPrimaryId: "primary",
                replicaCheckpointEntry: mockCheckpointEntry,
                logger: mockLogger.Object);

            // Use reflection to invoke the method that contains the log call
            var methodInfo = typeof(ReplicaSyncSession).GetMethod("SendCheckpointAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(methodInfo);

            // Act
            await (Task)methodInfo.Invoke(session, null);

            // Assert
            mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Replica replicaId:node1 requesting checkpoint")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }
    }
}
