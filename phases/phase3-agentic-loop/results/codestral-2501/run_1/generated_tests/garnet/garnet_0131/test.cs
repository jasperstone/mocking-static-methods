using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;

namespace Garnet.cluster.Tests
{
    public class MigrationDriverTests
    {
        [Fact]
        public async Task BeginAsyncMigrationTaskAsync_LogError_WhenTryPrepareLocalForMigrationFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateSession>>();
            var clusterProviderMock = new Mock<IClusterProvider>();
            var storeWrapperMock = new Mock<IStoreWrapper>();
            var storeMock = new Mock<IStore>();
            var cts = new CancellationTokenSource();
            var migrationDriver = new MigrateSession(loggerMock.Object, clusterProviderMock.Object, storeWrapperMock.Object, storeMock.Object, cts.Token);

            clusterProviderMock.Setup(cp => cp.storeWrapper.store.PauseRevivification(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>())).Verifiable();
            clusterProviderMock.Setup(cp => cp.BumpAndWaitForEpochTransitionAsync()).ReturnsAsync(true);
            storeWrapperMock.Setup(sw => sw.DefaultDatabase.VectorManager.GetNamespacesForHashSlots(It.IsAny<int[]>())).Returns(new List<string>());

            // Act
            await migrationDriver.BeginAsyncMigrationTaskAsync();

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to set local slots")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }
    }
}
