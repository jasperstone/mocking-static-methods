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
        public async Task AcquireCheckpointEntryAsync_LogsInformationWithIteration()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            var storeWrapperMock = new Mock<StoreWrapper>();

            // Setup minimal required properties and methods for clusterProvider and storeWrapper
            var replicationManagerMock = new Mock<ReplicationManager>();
            clusterProviderMock.SetupGet(c => c.replicationManager).Returns(replicationManagerMock.Object);

            // Setup TryGetLatestCheckpointEntryFromMemory to return true to avoid infinite loop
            replicationManagerMock.Setup(r => r.TryGetLatestCheckpointEntryFromMemory(out It.Ref<CheckpointEntry>.IsAny))
                .Returns(true)
                .Callback(new TryGetLatestCheckpointEntryFromMemoryDelegate((out CheckpointEntry entry) =>
                {
                    entry = new CheckpointEntry();
                }));

            var session = new ReplicaSyncSession(
                storeWrapperMock.Object,
                clusterProviderMock.Object,
                logger: loggerMock.Object);

            // Act
            var task = session.AcquireCheckpointEntryAsync();
            var result = await task;

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

        private delegate bool TryGetLatestCheckpointEntryFromMemoryDelegate(out CheckpointEntry entry);
    }
}
