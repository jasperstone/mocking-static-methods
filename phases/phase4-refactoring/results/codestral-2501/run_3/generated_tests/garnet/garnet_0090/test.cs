using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using System.Threading.Tasks;

public class MigrateOperationTests
{
    [Fact]
    public async Task TransmitSlotsAsync_Failure_LogsWarning()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<MigrateOperation>>();
        var migrateSession = new Mock<MigrateSession>();
        var migrateOperation = new MigrateOperation(migrateSession.Object);

        // Simulate a failure in TransmitSlotsAsync
        migrateOperation.sketch.argSliceVector.Add(new byte[] { 0x01 });

        // Act
        var result = await migrateOperation.TransmitSlotsAsync(StoreType.Main);

        // Assert
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
