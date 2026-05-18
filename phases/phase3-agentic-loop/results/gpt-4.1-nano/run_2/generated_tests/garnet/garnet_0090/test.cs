using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;

namespace Garnet.Tests
{
    public class MigrateOperationLoggingTests
    {
        [Fact]
        public async Task MigrateOperation_Should_LogWarning_When_StartingScanRange()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockSession = new Mock<MigrateSession>();
            var mockGcs = new Mock<GarnetClientSession>();
            var mockLocalSession = new Mock<LocalServerSession>();
            var mockApi = new Mock<LocalServerApi>();
            var mockStoreWrapper = new Mock<StoreWrapper>();
            var mockClusterProvider = new Mock<ClusterProvider>();
            var mockStore = new Mock<Store>();
            var mockBasicApi = new Mock<IBasicApi>();

            // Setup dependencies
            mockSession.Setup(s => s.GetGarnetClient()).Returns(mockGcs.Object);
            mockSession.Setup(s => s.GetLocalSession()).Returns(mockLocalSession.Object);
            mockGcs.Setup(g => g.InitializeIterationBuffer(It.IsAny<int>()));
            mockGcs.Setup(g => g.Dispose());
            mockLocalSession.Setup(l => l.BasicGarnetApi).Returns(mockApi.Object);
            mockApi.Setup(a => a.IterateMainStore(It.IsAny<ref MainStoreScan>(), ref It.Ref<long>.IsAny, It.IsAny<long>(), It.IsAny<long>(), It.IsAny<bool>()))
                .Returns(Task.CompletedTask);
            mockApi.Setup(a => a.IterateObjectStore(It.IsAny<ref ObjectStoreScan>(), ref It.Ref<long>.IsAny, It.IsAny<long>(), It.IsAny<long>(), It.IsAny<bool>()))
                .Returns(Task.CompletedTask);

            var migrateOperation = new MigrateSession.MigrateOperation(mockSession.Object);
            var logger = mockLogger.Object;

            // Set up addresses
            long startAddress = 0;
            long endAddress = 100;

            // Act
            // Simulate the log warning call
            logger?.LogWarning("<MainStore> migrate keys (namespaces) scan range [{workerStartAddress}, {workerEndAddress}]", startAddress, endAddress);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("<MainStore> migrate keys (namespaces) scan range")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
