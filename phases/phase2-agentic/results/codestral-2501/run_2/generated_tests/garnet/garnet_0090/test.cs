using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Garnet.Tests
{
    public class MigrateOperationTests
    {
        [Fact]
        public async Task TransmitSlotsAsync_Fails_LogsWarning()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockSession = new Mock<MigrateSession>(MockBehavior.Strict, null, null, null, 0, null, null, null, null, false, false, 0, null, null, TransferOption.SLOTS);
            var migrateOperation = new MigrateOperation(mockSession.Object, null, 1 << 18);
            migrateOperation.sketch.argSliceVector = new List<byte[]>() { new byte[] { 1, 2, 3 } };

            // Act
            var result = await migrateOperation.TransmitSlotsAsync(StoreType.Main);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("TransmitSlots failed for")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
            Assert.False(result);
        }
    }
}
