using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;

public class MigrationDriverTests
{
    [Fact]
    public async Task BeginAsyncMigrationTaskAsync_ShouldLogErrorWhenRelinquishOwnershipFails()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<MigrateSession>>();
        var clusterProviderMock = new Mock<ClusterProvider>();
        var clientMock = new Mock<GarnetClientSession>();
        var migrateOperationMock = new Mock<MigrateOperation>();

        var migrateSession = new MigrateSession(
            clusterSession: null,
            clusterProvider: clusterProviderMock.Object,
            _targetAddress: "targetAddress",
            _targetPort: 1234,
            _targetNodeId: "targetNodeId",
            _username: "username",
            _passwd: "password",
            _sourceNodeId: "sourceNodeId",
            _copyOption: false,
            _replaceOption: false,
            _timeout: 10000,
            _slots: new HashSet<int> { 1, 2, 3 },
            sketch: null,
            transferOption: TransferOption.SLOTS
        )
        {
            logger = loggerMock.Object,
            migrateOperation = new[] { migrateOperationMock.Object }
        };

        migrateOperationMock.Setup(x => x.Client).Returns(clientMock.Object);

        // Act
        await migrateSession.BeginAsyncMigrationTaskAsync();

        // Assert
        loggerMock.Verify(
            x => x.LogError(
                "Failed to relinquish ownership from source node:({srcNode}) to target node: ({tgtNode})",
                It.IsAny<string>(),
                It.IsAny<string>()),
            Times.Once);
    }
}
