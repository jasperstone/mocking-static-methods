using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class MigrateOperationTests
    {
        [Fact]
        public async Task TransmitSlotsAsync_LogsWarningWhenFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var migrateOperation = new MigrateOperation(new MigrateSession(), new Sketch());
            migrateOperation.logger = loggerMock.Object;

            // Act
            var result = await migrateOperation.TransmitSlotsAsync(StoreType.Main);

            // Assert
            loggerMock.Verify(l => l.LogWarning("TransmitSlots failed for {cursor} to {current} (with {count} keys)", It.IsAny<long>(), It.IsAny<long>(), It.IsAny<int>()), Times.Once);
            Assert.False(result);
        }
    }
}
