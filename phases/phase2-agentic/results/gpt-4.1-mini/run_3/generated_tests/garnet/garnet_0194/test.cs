using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.cluster;
using System.Threading;
using System.Net;
using System;

namespace Garnet.cluster.Tests
{
    public class ReplicaSyncSessionTests
    {
        [Fact]
        public async Task AcquireCheckpointEntryAsync_LogsInformationWithIteration()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            var storeWrapperMock = new Mock<StoreWrapper>();

            // Setup clusterProviderMock to simulate TryGetLatestCheckpointEntryFromMemory returning true once
            var replicationManagerMock = new Mock<ReplicationManager>();
            CheckpointEntry dummyCheckpointEntry = new CheckpointEntry();
            replicationManagerMock.Setup(rm => rm.TryGetLatestCheckpointEntryFromMemory(out dummyCheckpointEntry))
                .Returns(true);
            clusterProviderMock.SetupGet(cp => cp.replicationManager).Returns(replicationManagerMock.Object);

            // Setup storeWrapperMock to have lastSaveTime property
            storeWrapperMock.SetupGet(sw => sw.lastSaveTime).Returns(DateTime.UtcNow.Ticks);

            // Create the ReplicaSyncSession instance with mocks
            var session = new ReplicaSyncSession(
                storeWrapperMock.Object,
                clusterProviderMock.Object,
                logger: loggerMock.Object);

            // Act
            // We call the internal method AcquireCheckpointEntryAsync via reflection because it's private
            var method = typeof(ReplicaSyncSession).GetMethod("AcquireCheckpointEntryAsync", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            var task = (Task<(CheckpointEntry, AofSyncTaskInfo)>)method.Invoke(session, null);
            await task;

            // Assert
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
