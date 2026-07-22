using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

public class MigrateOperationTests
{
    [Fact]
    public async Task TransmitSlotsAsync_Failure_LogsWarning()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<MigrateOperation>>();
        var mockSession = new Mock<MigrateSession>();
        var mockSketch = new Mock<Sketch>();
        var migrateOperation = new MigrateOperation(mockSession.Object, mockSketch.Object);

        // Set up the mocks to simulate a failure in TransmitSlotsAsync
        mockSketch.Setup(s => s.argSliceVector).Returns(new List<byte[]>() { new byte[] { 1, 2, 3 } });
        mockSession.Setup(s => s.WriteOrSendMainStoreKeyValuePairAsync(It.IsAny<GarnetClientSession>(), It.IsAny<LocalServerSession>(), ref It.Ref<byte[]>.IsAny, ref It.Ref<RawStringInput>.IsAny, ref It.Ref<SpanByteAndMemory>.IsAny, out It.Ref<GarnetStatus>.IsAny)).ReturnsAsync(false);

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
