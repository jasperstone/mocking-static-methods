using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using System.Threading.Tasks;
using System.Collections.Generic;
using Garnet.client;
using Garnet.server;
using Tsavorite.core;

namespace Garnet.Tests
{
    public class MigrateOperationTests
    {
        [Fact]
        public async Task TransmitSlotsAsync_Failure_LogsWarning()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<MigrateOperation>>();
            var mockSession = new Mock<MigrateSession>(MockBehavior.Strict, null, null, null, 0, null, null, null, null, false, false, 0, null, null, TransferOption.SLOTS);
            var migrateOperation = new MigrateOperation(mockSession.Object, new Sketch(100));

            // Mock the necessary methods and properties
            mockSession.Setup(s => s.WriteOrSendMainStoreKeyValuePairAsync(It.IsAny<GarnetClientSession>(), It.IsAny<LocalServerSession>(), ref It.Ref<SpanByte>.IsAny, ref It.Ref<RawStringInput>.IsAny, ref It.Ref<SpanByteAndMemory>.IsAny, out It.Ref<GarnetStatus>.IsAny)).ReturnsAsync(false);
            mockSession.Setup(s => s.HandleMigrateTaskResponseAsync(It.IsAny<Task<GarnetStatus>>())).ReturnsAsync(true);

            // Act
            var result = await migrateOperation.TransmitSlotsAsync(StoreType.Main);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);

            Assert.False(result);
        }
    }
}
