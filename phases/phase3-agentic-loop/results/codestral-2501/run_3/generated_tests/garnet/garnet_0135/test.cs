using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Garnet.cluster;
using System.Threading;
using System;

public class MigrationDriverTests
{
    [Fact]
    public async Task BeginAsyncMigrationTaskAsync_ShouldLogError_WhenRelinquishOwnershipFails()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<MigrateSession>>();
        var clusterProviderMock = new Mock<ClusterProvider>();
        var cts = new CancellationTokenSource();
        var timeout = TimeSpan.FromSeconds(10);
        var targetAddress = "127.0.0.1";
        var targetPort = 6379;
        var sourceNodeId = "sourceNode";
        var targetNodeId = "targetNode";
        var slotRanges = new string[] { "0-16383" };
        var sslots = new string[] { "0-16383" };
        var transferOption = TransferOption.SLOTS;
        var migrateOperation = new MigrateOperation[] { new MigrateOperation() };

        var migrateSession = new MigrateSession(
            loggerMock.Object,
            clusterProviderMock.Object,
            cts,
            timeout,
            targetAddress,
            targetPort,
            sourceNodeId,
            targetNodeId,
            slotRanges,
            sslots,
            transferOption,
            migrateOperation
        );

        // Mock the necessary methods to simulate failure
        migrateSession.RelinquishOwnership = () => false;

        // Act
        await migrateSession.BeginAsyncMigrationTaskAsync();

        // Assert
        loggerMock.Verify(
            logger => logger.LogError(
                "Failed to relinquish ownership from source node:({srcNode}) to target node: ({tgtNode})",
                sourceNodeId,
                targetNodeId),
            Times.Once);
    }
}
