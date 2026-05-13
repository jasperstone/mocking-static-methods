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
            var loggerMock = new Mock<ILogger<MigrateOperation>>();
            var sessionMock = new Mock<MigrateSession>();
            var sketchMock = new Mock<Sketch>();
            var migrateOperation = new MigrateOperation(sessionMock.Object, sketchMock.Object);

            sessionMock.Setup(s => s.WriteOrSendMainStoreKeyValuePairAsync(It.IsAny<GarnetClientSession>(), It.IsAny<LocalServerSession>(), ref It.Ref<SpanByte>.IsAny, ref It.Ref<RawStringInput>.IsAny, ref It.Ref<SpanByteAndMemory>.IsAny, out It.Ref<GarnetStatus>.IsAny))
                .ReturnsAsync(false);

            sketchMock.Setup(s => s.argSliceVector).Returns(new List<SpanByte> { new SpanByte(new byte[] { 1, 2, 3 }) });

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
