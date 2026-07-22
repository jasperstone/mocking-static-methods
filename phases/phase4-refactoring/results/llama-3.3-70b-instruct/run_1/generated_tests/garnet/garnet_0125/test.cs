using Xunit;
using Moq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Garnet.cluster;

namespace Garnet.cluster
{
    public class MigrationDriverTests
    {
        [Fact]
        public async Task TrySetSlotRangesAsync_LogsTraceMessage_WhenSuccessful()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateSession>>();
            var migrationDriver = new MigrateSession(loggerMock.Object);
            var clientMock = new Mock<IClient>();
            migrationDriver.migrateOperation = new[] { new MigrateOperation { Client = clientMock.Object } };
            migrationDriver._slotRanges = new[] { 1, 2, 3 };
            migrationDriver._timeout = TimeSpan.FromSeconds(1);
            migrationDriver._cts = new CancellationTokenSource();

            // Act
            var result = await migrationDriver.TrySetSlotRangesAsync("nodeid", MigrateState.IMPORT);

            // Assert
            loggerMock.Verify(l => l.LogTrace("Sending CLUSTER SETSLOTRANGE {state} {nodeid} {slots}", MigrateState.IMPORT, "nodeid", "1-3"), Times.Once);
            loggerMock.Verify(l => l.LogTrace("[Completed] SETSLOT {slots} {state} {nodeid}", "1-3", MigrateState.IMPORT, "nodeid"), Times.Once);
        }

        [Fact]
        public async Task TrySetSlotRangesAsync_LogsErrorMessage_WhenFailed()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateSession>>();
            var migrationDriver = new MigrateSession(loggerMock.Object);
            var clientMock = new Mock<IClient>();
            migrationDriver.migrateOperation = new[] { new MigrateOperation { Client = clientMock.Object } };
            migrationDriver._slotRanges = new[] { 1, 2, 3 };
            migrationDriver._timeout = TimeSpan.FromSeconds(1);
            migrationDriver._cts = new CancellationTokenSource();
            clientMock.Setup(c => c.SetSlotRange(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<int[]>())).Returns(Task.FromResult("ERROR"));

            // Act
            var result = await migrationDriver.TrySetSlotRangesAsync("nodeid", MigrateState.IMPORT);

            // Assert
            loggerMock.Verify(l => l.LogError("SetSlotRange error: {error}", "ERROR"), Times.Once);
        }
    }
}
