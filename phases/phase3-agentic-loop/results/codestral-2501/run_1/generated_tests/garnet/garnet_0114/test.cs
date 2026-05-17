using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Garnet.client;
using Garnet.cluster;
using Garnet.server;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class MigrateSessionSlotsTests
{
    [Fact]
    public async Task CreateAndRunMigrateTasksAsync_ExceptionLogged()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<MigrateSession>>();
        var migrateOperationMock = new Mock<MigrateSession.MigrateOperation>();
        var clusterProviderMock = new Mock<ClusterProvider>();
        var cts = new CancellationTokenSource();

        var migrateSession = new MigrateSession(
            clusterSession: null,
            clusterProvider: clusterProviderMock.Object,
            _targetAddress: "127.0.0.1",
            _targetPort: 6379,
            _targetNodeId: "targetNode",
            _username: "user",
            _passwd: "pass",
            _sourceNodeId: "sourceNode",
            _copyOption: false,
            _replaceOption: false,
            _timeout: 1000,
            _slots: new HashSet<int> { 1 },
            sketch: null,
            transferOption: TransferOption.SLOTS
        );

        migrateSession.migrateOperation[0] = migrateOperationMock.Object;

        // Simulate an exception during the migration process
        migrateOperationMock.Setup(mo => mo.InitializeAsync()).ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await migrateSession.CreateAndRunMigrateTasksAsync(StoreType.Main, 0, 100, 10);

        // Assert
        loggerMock.Verify(
            logger => logger.LogError(
                It.IsAny<Exception>(),
                "{CreateAndRunMigrateTasks}: {storeType} {beginAddress} {tailAddress} {pageSize}",
                nameof(migrateSession.CreateAndRunMigrateTasksAsync),
                StoreType.Main,
                0,
                100,
                10
            ),
            Times.Once
        );

        Assert.False(result);
    }
}
