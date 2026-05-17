using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Reflection;
using Garnet.cluster;

namespace Garnet.Tests
{
    public class MigrateOperationTests
    {
        [Fact]
        public async Task TransmitSlotsAsync_LogsWarning_WhenTransmissionFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var migrateSession = new MigrateSession(
                new ClusterSession(),
                new ClusterProvider(),
                "targetAddress",
                1234,
                "targetNodeId",
                "username",
                "password",
                "sourceNodeId",
                true,
                true,
                1000,
                new HashSet<int>(),
                new Sketch(),
                TransferOption.SLOTS
            );
            var migrateOperation = migrateSession.GetType().GetField("migrateOperation", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(migrateSession) as MigrateOperation[];
            migrateOperation[0].logger = loggerMock.Object;

            // Act
            var result = await migrateOperation[0].TransmitSlotsAsync(StoreType.Main);

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task TransmitSlotsAsync_DoesNotLogWarning_WhenTransmissionSucceeds()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var migrateSession = new MigrateSession(
                new ClusterSession(),
                new ClusterProvider(),
                "targetAddress",
                1234,
                "targetNodeId",
                "username",
                "password",
                "sourceNodeId",
                true,
                true,
                1000,
                new HashSet<int>(),
                new Sketch(),
                TransferOption.SLOTS
            );
            var migrateOperation = migrateSession.GetType().GetField("migrateOperation", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(migrateSession) as MigrateOperation[];
            migrateOperation[0].logger = loggerMock.Object;

            // Act
            var result = await migrateOperation[0].TransmitSlotsAsync(StoreType.Main);

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
        }
    }
}
