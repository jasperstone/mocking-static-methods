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
            var loggerMock = new Mock<ILogger>();
            var migrateOperation = new MigrateOperation(new MigrateSession(), new Sketch());
            migrateOperation.logger = loggerMock.Object;

            // Act
            var result = await migrateOperation.TransmitSlotsAsync(StoreType.Main);

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task LogWarning_Called_With_Correct_Message_When_TransmitSlotsAsync_Fails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var migrateOperation = new MigrateOperation(new MigrateSession(), new Sketch());
            migrateOperation.logger = loggerMock.Object;
            migrateOperation.sketch.argSliceVector.Add(new byte[] { 1, 2, 3 });
            migrateOperation.sketch.argSliceVector.Add(new byte[] { 4, 5, 6 });

            // Act
            var result = await migrateOperation.TransmitSlotsAsync(StoreType.Main);

            // Assert
            loggerMock.Verify(l => l.LogWarning("TransmitSlots failed for {cursor} to {current} (with {count} keys)", It.IsAny<long>(), It.IsAny<long>(), It.IsAny<int>()), Times.Once);
        }
    }
}
