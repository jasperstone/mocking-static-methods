using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class ReplicaSyncSessionTests
    {
        [Fact]
        public async Task AcquireCheckpointEntry_LogsInformationOnIteration()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var storeWrapper = new Mock<StoreWrapper>();
            var clusterProvider = new Mock<ClusterProvider>();
            var replicaSyncMetadata = new SyncMetadata();
            var token = CancellationToken.None;

            var session = new ReplicaSyncSession(
                storeWrapper.Object,
                clusterProvider.Object,
                replicaSyncMetadata,
                token,
                logger: mockLogger.Object);

            // Act
            await session.AcquireCheckpointEntryAsync();

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.Is<EventId>(e => e.Id == 0), // Assuming EventId is 0 for LogInformation
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("AcquireCheckpointEntry iteration")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }
    }
}
