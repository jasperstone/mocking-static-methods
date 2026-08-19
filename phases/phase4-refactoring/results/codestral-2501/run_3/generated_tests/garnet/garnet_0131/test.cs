using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Collections.Generic;
using Garnet.cluster;

namespace Garnet.cluster.Tests
{
    public class MigrationDriverTests
    {
        [Fact]
        public async Task BeginAsyncMigrationTaskAsync_FailsToSetLocalSlotsToMigrateState_LogsError()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<MigrateSession>>();
            var mockClusterProvider = new Mock<ClusterProvider>();
            var mockStoreWrapper = new Mock<StoreWrapper>();
            var mockStore = new Mock<Store>();
            var mockClusterManager = new Mock<ClusterManager>();
            var mockVectorManager = new Mock<VectorManager>();
            var mockMigrateSession = new Mock<MigrateSession>(mockClusterProvider.Object, mockStoreWrapper.Object, mockClusterManager.Object, mockVectorManager.Object, CancellationToken.None);

            mockMigrateSession.Setup(x => x.TryPrepareLocalForMigration()).Returns(false);
            mockMigrateSession.Setup(x => x.GetSlots()).Returns(new HashSet<int> { 1, 2, 3 });
            mockMigrateSession.Setup(x => x.TryRecoverFromFailureAsync()).Returns(Task.FromResult(true));
            mockMigrateSession.Setup(x => x.Status).Returns(MigrateState.FAIL);

            // Act
            await mockMigrateSession.Object.BeginAsyncMigrationTaskAsync();

            // Assert
            mockLogger.Verify(
                x => x.LogError(
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
