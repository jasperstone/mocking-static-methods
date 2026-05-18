using Xunit;
using Moq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Reflection;
using Garnet.cluster;

namespace Garnet.Tests
{
    public class ReplicaSyncSessionTests
    {
        [Fact]
        public async Task AcquireCheckpointEntryAsync_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            var clusterProviderMock = new Mock<ClusterProvider>(storeWrapperMock.Object);
            var replicaSyncSession = Activator.CreateInstance(typeof(ReplicaSyncSession), storeWrapperMock.Object, clusterProviderMock.Object, logger: loggerMock.Object) as ReplicaSyncSession;

            // Act
            var acquireCheckpointEntryAsyncMethod = typeof(ReplicaSyncSession).GetMethod("AcquireCheckpointEntryAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            await (Task)acquireCheckpointEntryAsyncMethod.Invoke(replicaSyncSession, null);

            // Assert
            loggerMock.Verify(l => l.Log(It.Is<LogLevel>(ll => ll == LogLevel.Information), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.AtLeastOnce);
        }
    }
}
