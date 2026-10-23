using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using Garnet.client;
using Garnet.server;
using Tsavorite.core;

public class MigrateOperationTests
{
    [Fact]
    public async Task TransmitSlotsAsync_Failure_LogsWarning()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<MigrateOperation>>();
        var migrateSession = new Mock<MigrateSession>();
        var migrateOperation = new MigrateOperation(migrateSession.Object);

        // Mock the necessary methods and properties
        migrateOperation.sketch.argSliceVector.Add(new byte[] { 0x01 });
        migrateSession.Setup(s => s.WriteOrSendMainStoreKeyValuePairAsync(It.IsAny<GarnetClientSession>(), It.IsAny<LocalServerSession>(), ref It.Ref<SpanByte>.IsAny, ref It.Ref<RawStringInput>.IsAny, ref It.Ref<SpanByteAndMemory>.IsAny, out It.Ref<GarnetStatus>.IsAny)).ReturnsAsync(false);

        // Act
        var result = await migrateOperation.TransmitSlotsAsync(StoreType.Main);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("TransmitSlots failed")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);

        Assert.False(result);
    }
}
