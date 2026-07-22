using Microsoft.Extensions.Logging;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace Garnet.cluster
{
    public class ReplicaSyncSessionTests
    {
        [Fact]
        public async Task AcquireCheckpointEntryAsync_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var storeWrapper = new StoreWrapper();
            var serverOptions = new ServerOptions();
            var clusterProvider = new ClusterProvider(storeWrapper, serverOptions, loggerMock.Object);
            var replicaSyncSession = new ReplicaSyncSession(
                storeWrapper,
                clusterProvider,
                logger: loggerMock.Object);

            // Act
            await replicaSyncSession.AcquireCheckpointEntryAsync();

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object>()), Times.AtLeastOnce);
        }
    }
}
