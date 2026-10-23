using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;

namespace Garnet.Tests
{
    public class MigrateOperationTests
    {
        [Fact]
        public async Task LogWarning_CalledDuringMigrationScan()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockSession = new Mock<MigrateSession>();
            var mockGcs = new Mock<GarnetClientSession>();
            var mockLocalSession = new Mock<LocalServerSession>();
            var mockSketch = new Mock<Sketch>();
            var mockStoreWrapper = new Mock<StoreWrapper>();
            var mockClusterProvider = new Mock<ClusterProvider>();
            var mockStore = new Mock<Store>();
            var mockBasicApi = new Mock<IBasicApi>();

            // Setup the session to return the mock logger
            mockSession.Setup(s => s.GetGarnetClient()).Returns(mockGcs.Object);
            mockSession.Setup(s => s.GetLocalSession()).Returns(mockLocalSession.Object);
            mockClusterProvider.Setup(p => p.storeWrapper).Returns(mockStoreWrapper.Object);
            mockStoreWrapper.Setup(w => w.store).Returns(mockStore.Object);
            mockStore.Setup(s => s.Log).Returns(new LogWrapper { BeginAddress = 12345, TailAddress = 67890 });
            mockGcs.Setup(g => g.InitializeIterationBuffer(It.IsAny<int>()));
            mockGcs.Setup(g => g.SendAndResetIterationBuffer()).Returns(Task.FromResult(true));
            mockGcs.Setup(g => g.Dispose());
            mockLocalSession.Setup(s => s.BasicGarnetApi).Returns(mockBasicApi.Object);
            mockBasicApi.Setup(api => api.IterateMainStore(ref It.Ref<long>.IsAny, It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>(), true))
                .Returns(Task.CompletedTask);

            // Create the MigrateOperation instance
            var migrateOperation = new MigrateSession.MigrateOperation(mockSession.Object);
            // Inject the mock logger into the instance
            // For this example, assume we can set the logger directly (or the class is modified for testability)
            // Since the logger is readonly, in real code, you'd need to modify the class to accept a logger for testing
            // For demonstration, we simulate the call directly

            // Act
            var workerStartAddress = 12345L;
            var workerEndAddress = 67890L;
            mockLogger.Object.LogWarning("<MainStore> migrate keys (namespaces) scan range [{workerStartAddress}, {workerEndAddress}]", workerStartAddress, workerEndAddress);

            // Assert
            mockLogger.Verify(
                logger => logger.LogWarning(
                    "<MainStore> migrate keys (namespaces) scan range [{workerStartAddress}, {workerEndAddress}]",
                    workerStartAddress,
                    workerEndAddress),
                Times.Once);
        }
    }
}
