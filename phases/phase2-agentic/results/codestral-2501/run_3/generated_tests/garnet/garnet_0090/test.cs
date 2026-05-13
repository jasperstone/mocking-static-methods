using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace Garnet.Tests
{
    public class MigrateOperationTests
    {
        [Fact]
        public async Task TransmitSlotsAsync_Fails_LogsWarning()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var sessionMock = new Mock<MigrateSession>();
            var migrateOperation = new MigrateOperation(sessionMock.Object, sketch: new Sketch());

            var cursor = 0L;
            var current = 10L;
            var count = 5;

            migrateOperation.sketch.argSliceVector = new List<byte[]>
            {
                new byte[] { 1, 2, 3 },
                new byte[] { 4, 5, 6 },
                new byte[] { 7, 8, 9 },
                new byte[] { 10, 11, 12 },
                new byte[] { 13, 14, 15 }
            };

            sessionMock.Setup(s => s.WriteOrSendMainStoreKeyValuePairAsync(It.IsAny<GarnetClientSession>(), It.IsAny<LocalServerSession>(), ref It.Ref<SpanByte>.IsAny, ref It.Ref<RawStringInput>.IsAny, ref It.Ref<SpanByteAndMemory>.IsAny, out It.Ref<GarnetStatus>.IsAny))
                .ReturnsAsync(false);

            // Act
            var result = await migrateOperation.TransmitSlotsAsync(StoreType.Main);

            // Assert
            loggerMock.Verify(
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
