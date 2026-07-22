using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Garnet.cluster;

public class MigrationDriverTests
{
    [Fact]
    public async Task TrySetSlotRangesAsync_LogsErrorOnTimeout()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<MigrateSession>>();
        var mockClusterSession = new Mock<ClusterSession>();
        var mockClusterProvider = new Mock<ClusterProvider>();
        var mockGarnetClientSession = new Mock<GarnetClientSession>();

        var migrateSession = new MigrateSession(
            clusterSession: mockClusterSession.Object,
            clusterProvider: mockClusterProvider.Object,
            _targetAddress: "127.0.0.1",
            _targetPort: 6379,
            _targetNodeId: "targetNodeId",
            _username: "username",
            _passwd: "password",
            _sourceNodeId: "sourceNodeId",
            _copyOption: false,
            _replaceOption: false,
            _timeout: 100,
            _slots: new HashSet<int> { 1, 2, 3 },
            sketch: null,
            transferOption: TransferOption.SLOTS
        )
        {
            logger = mockLogger.Object,
            _timeout = TimeSpan.FromMilliseconds(100),
            _sslots = new HashSet<int> { 1, 2, 3 }
        };

        mockGarnetClientSession.Setup(client => client.SetSlotRange(It.IsAny<Memory<byte>>(), It.IsAny<string>(), It.IsAny<List<(int, int)>>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act
        var result = await migrateSession.TrySetSlotRangesAsync("nodeid", MigrateState.IMPORT);

        // Assert
        mockLogger.Verify(
            logger => logger.LogError(
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("SetSlotRange operation timed out or was cancelled")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
        Assert.False(result);
    }
}
