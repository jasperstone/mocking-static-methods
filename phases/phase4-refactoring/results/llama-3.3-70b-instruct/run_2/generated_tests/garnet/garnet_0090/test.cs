using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;

public class MigrateOperationTests
{
    [Fact]
    public void LogWarning_CalledWithCorrectArguments()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var migrateSessionMock = new Mock<MigrateSession>();
        var sketchMock = new Mock<Sketch>();
        var migrateOperation = new MigrateOperation(migrateSessionMock.Object, sketchMock.Object);
        migrateOperation.logger = loggerMock.Object;

        // Act
        migrateOperation.logger.LogWarning("<MainStore> migrate keys (namespaces) scan range [{workerStartAddress}, {workerEndAddress}]", 1, 2);

        // Assert
        loggerMock.Verify(l => l.LogWarning("<MainStore> migrate keys (namespaces) scan range [{workerStartAddress}, {workerEndAddress}]", 1, 2), Times.Once);
    }

    [Fact]
    public void LogWarning_TransmitSlotsFailed_CalledWithCorrectArguments()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var migrateSessionMock = new Mock<MigrateSession>();
        var sketchMock = new Mock<Sketch>();
        var migrateOperation = new MigrateOperation(migrateSessionMock.Object, sketchMock.Object);
        migrateOperation.logger = loggerMock.Object;

        // Act
        migrateOperation.logger.LogWarning("TransmitSlots failed for {cursor} to {current} (with {count} keys)", 1, 2, 3);

        // Assert
        loggerMock.Verify(l => l.LogWarning("TransmitSlots failed for {cursor} to {current} (with {count} keys)", 1, 2, 3), Times.Once);
    }
}
