using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using Garnet.server;
using Tsavorite.core;

public class MigrateOperationTests
{
    [Fact]
    public async Task TransmitSlotsAsync_Failure_LogsWarning()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<MigrateOperation>>();
        var mockSession = new Mock<MigrateSession>(MockBehavior.Strict, null, null, null, 0, null, null, null, null, false, false, 0, null, null, TransferOption.SLOTS);
        var migrateOperation = new MigrateOperation(mockSession.Object);

        // Set up the sketch to simulate failure
        migrateOperation.sketch.argSliceVector.Add(new ArgSlice());

        // Act
        var result = await migrateOperation.TransmitSlotsAsync(StoreType.Main);

        // Assert
        Assert.False(result);
        mockLogger.Verify(
            logger => logger.LogWarning(
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
