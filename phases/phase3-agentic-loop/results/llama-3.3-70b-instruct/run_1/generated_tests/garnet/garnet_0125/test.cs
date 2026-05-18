using Xunit;
using Moq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Garnet.cluster
{
    public class MigrateSessionTests
    {
        [Fact]
        public async Task TrySetSlotRangesAsync_LogTrace_Called()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var migrateSession = new MigrateSession(loggerMock.Object, null, null, null, null, null, null);
            var nodeid = "nodeid";
            var state = MigrateState.IMPORT;

            // Act
            await migrateSession.TrySetSlotRangesAsync(nodeid, state);

            // Assert
            loggerMock.Verify(l => l.LogTrace("Sending CLUSTER SETSLOTRANGE {state} {nodeid} {slots}", state, nodeid ?? "null", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task TrySetSlotRangesAsync_LogTrace_Completed_Called()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var migrateSession = new MigrateSession(loggerMock.Object, null, null, null, null, null, null);
            var nodeid = "nodeid";
            var state = MigrateState.IMPORT;

            // Act
            await migrateSession.TrySetSlotRangesAsync(nodeid, state);

            // Assert
            loggerMock.Verify(l => l.LogTrace("[Completed] SETSLOT {slots} {state} {nodeid}", It.IsAny<string>(), state, nodeid ?? ""), Times.Once);
        }
    }
}
