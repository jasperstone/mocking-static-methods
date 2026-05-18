using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.cluster;
using System.Threading;
using System.Net;
using Garnet.client;
using System;

namespace Garnet.cluster.Tests
{
    public class ReplicaSyncSessionTests
    {
        [Fact]
        public async Task AcquireCheckpointEntryAsync_LogsInformationOnEachIteration()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterProviderMock = new Mock<ClusterProvider>(MockBehavior.Strict);
            var storeWrapperMock = new Mock<StoreWrapper>(MockBehavior.Strict);

            // Setup minimal required properties and methods for clusterProvider and storeWrapper
            var replicationManagerMock = new Mock<ReplicationManager>(MockBehavior.Strict);
            clusterProviderMock.SetupGet(c => c.replicationManager).Returns(replicationManagerMock.Object);

            // Setup TryGetLatestCheckpointEntryFromMemory to return false first, then true to break the loop
            int callCount = 0;
            replicationManagerMock.Setup(r => r.TryGetLatestCheckpointEntryFromMemory(out It.Ref<CheckpointEntry>.IsAny))
                .Returns(() =>
                {
                    callCount++;
                    return callCount > 1;
                });

            // Setup TryRemoveReplicationTask to do nothing
            replicationManagerMock.Setup(r => r.TryRemoveReplicationTask(It.IsAny<AofSyncTaskInfo>())).Returns(true);

            // Setup storeWrapper lastSaveTime property
            var storeWrapper = new StoreWrapper();
            // We don't need to set lastSaveTime for this test, so leave default

            // Create a ReplicaSyncSession instance with the mocked logger and clusterProvider
            var replicaSyncSession = new ReplicaSyncSession(
                storeWrapper,
                clusterProviderMock.Object,
                logger: loggerMock.Object);

            // Act
            // Call AcquireCheckpointEntryAsync once, which should log the iteration 0 message
            var task = replicaSyncSession.AcquireCheckpointEntryAsync();
            // Await the task to completion
            var result = await task;

            // Assert
            // Verify that LogInformation was called with the expected message containing iteration 0
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("AcquireCheckpointEntry iteration 0")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }
    }
}
