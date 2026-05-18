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
            var loggerMock = new Mock<ILogger>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            var replicaSyncSession = new ReplicaSyncSession(
                storeWrapperMock.Object,
                clusterProviderMock.Object,
                logger: loggerMock.Object);

            // Act
            await replicaSyncSession.AcquireCheckpointEntryAsync();

            // Assert
            loggerMock.Verify(
                x => x.LogInformation(
                    It.Is<string>(s => s.Contains("AcquireCheckpointEntry iteration")),
                    It.IsAny<int>()),
                Times.AtLeastOnce);
        }
    }
}
