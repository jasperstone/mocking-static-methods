using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Garnet.cluster;

namespace Garnet.Tests
{
    public class MigrateOperationTests
    {
        [Fact]
        public async Task LogWarning_Called_When_TransmitSlotsAsync_Fails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateOperation>>();
            var migrateOperation = new MigrateOperation(new MigrateSession(), new Sketch());
            migrateOperation.logger = loggerMock.Object;

            // Act
            var result = await migrateOperation.TransmitSlotsAsync(StoreType.Main);

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public async Task LogWarning_Called_When_Scan_Range_Is_Set()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateOperation>>();
            var migrateOperation = new MigrateOperation(new MigrateSession(), new Sketch());
            migrateOperation.logger = loggerMock.Object;

            // Act
            var workerStartAddress = 0L;
            var workerEndAddress = 10L;
            migrateOperation.Scan(StoreType.Main, ref workerStartAddress, workerEndAddress);

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<long>()), Times.Once);
        }
    }
}
