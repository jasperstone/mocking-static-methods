using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Threading;
using System;
using Garnet.cluster;
using Garnet.client;

public class MigrationDriverTests
{
    [Fact]
    public async Task TrySetSlotRangesAsync_LogsTraceOnSuccess()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<MigrateSession>>();
        var mockClient = new Mock<GarnetClientSession>();
        var mockClusterManager = new Mock<IClusterManager>();
        var mockCts = new CancellationTokenSource();

        var migrateOperation = new MigrateOperation[] { new MigrateOperation { Client = mockClient.Object } };
        var migrateSession = new MigrateSession(migrateOperation, mockLogger.Object, mockClusterManager.Object, mockCts.Token);

        mockClient.Setup(client => client.SetSlotRange(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<SlotRange[]>()))
            .ReturnsAsync("OK");

        // Act
        var result = await migrateSession.TrySetSlotRangesAsync("nodeid", MigrateState.IMPORT);

        // Assert
        mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Sending CLUSTER SETSLOTRANGE")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);

        mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("[Completed] SETSLOT")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);

        Assert.True(result);
    }
}
