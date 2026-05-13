using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Garnet.cluster;
using Garnet.server;
using Garnet.client;
using Tsavorite.core;

public class MigrateSessionSlotsTests
{
    [Fact]
    public async Task CreateAndRunMigrateTasksAsync_LogsErrorOnException()
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
        var mockLocalServerSession = new Mock<LocalServerSession>();

        mockClusterProvider.Setup(cp => cp.storeWrapper).Returns(mockStoreWrapper.Object);
        mockStoreWrapper.Setup(sw => sw.store).Returns(mockStore.Object);
        mockStoreWrapper.Setup(sw => sw.objectStore).Returns(mockObjectStore.Object);
        mockStoreWrapper.Setup(sw => sw.serverOptions).Returns(mockServerOptions.Object);
        mockMigrateOperation.Setup(mo => mo.Client).Returns(mockGarnetClientSession.Object);
        mockMigrateOperation.Setup(mo => mo.InitializeAsync()).ReturnsAsync(true);
        mockMigrateOperation.Setup(mo => mo.Scan(It.IsAny<StoreType>(), ref It.Ref<long>.IsAny, It.IsAny<long>()));
        mockMigrateOperation.Setup(mo => mo.TransmitSlotsAsync(It.IsAny<StoreType>())).ReturnsAsync(true);

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
            _slots: new System.Collections.Generic.HashSet<int> { 1 },
            sketch: null,
            transferOption: TransferOption.SLOTS
        );

        var migrateOperation = new MigrateOperation(migrateSession);
        migrateSession.migrateOperation[0] = migrateOperation;

        // Act
        var result = await migrateSession.CreateAndRunMigrateTasksAsync(StoreType.Main, 0, 100, 10);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }
}
