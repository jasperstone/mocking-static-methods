using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Garnet.cluster;
using Garnet.server;
using Garnet.client;
using System.Collections.Generic;
using System.Threading;

namespace Garnet.cluster.Tests
{
    public class MigrateSessionSlotsTests
    {
        [Fact]
        public async Task CreateAndRunMigrateTasksAsync_ShouldLogError_WhenExceptionIsThrown()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<MigrateSession>>();
            var mockClusterProvider = new Mock<ClusterProvider>();
            var mockStoreWrapper = new Mock<StoreWrapper>();
            var mockStore = new Mock<Store>();
            var mockObjectStore = new Mock<ObjectStore>();
            var mockServerOptions = new Mock<ServerOptions>();
            var mockMigrateOperation = new Mock<MigrateOperation>();
            var mockGarnetClientSession = new Mock<GarnetClientSession>();
            var mockCancellationTokenSource = new Mock<CancellationTokenSource>();

            mockClusterProvider.Setup(cp => cp.storeWrapper).Returns(mockStoreWrapper.Object);
            mockStoreWrapper.Setup(sw => sw.store).Returns(mockStore.Object);
            mockStoreWrapper.Setup(sw => sw.objectStore).Returns(mockObjectStore.Object);
            mockClusterProvider.Setup(cp => cp.serverOptions).Returns(mockServerOptions.Object);
            mockMigrateOperation.Setup(mo => mo.Client).Returns(mockGarnetClientSession.Object);

            var migrateSession = new MigrateSession(
                clusterSession: null,
                clusterProvider: mockClusterProvider.Object,
                _targetAddress: "127.0.0.1",
                _targetPort: 6379,
                _targetNodeId: "targetNodeId",
                _username: "username",
                _passwd: "password",
                _sourceNodeId: "sourceNodeId",
                _copyOption: false,
                _replaceOption: false,
                _timeout: 1000,
                _slots: new HashSet<int> { 1, 2, 3 },
                sketch: null,
                transferOption: TransferOption.SLOTS
            );

            var migrateOperation = new MigrateOperation(migrateSession);
            migrateSession.migrateOperation[0] = migrateOperation;

            var exception = new Exception("Test exception");

            // Act
            var result = await migrateSession.CreateAndRunMigrateTasksAsync(StoreType.Main, 0, 100, 10);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Test exception")),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
