using Xunit;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Garnet.server;

public class SingleDatabaseManagerTests
{
    [Fact]
    public async Task TaskCheckpointBasedOnAofSizeLimitAsync_AofSizeLessThanOrEqual_AofSizeLimit_LogInformationNotCalled()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var singleDatabaseManager = new SingleDatabaseManager(null, null, false);

        // Act
        await singleDatabaseManager.TaskCheckpointBasedOnAofSizeLimitAsync(100, logger: loggerMock.Object);

        // Assert
        loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
    }

    [Fact]
    public async Task TaskCheckpointBasedOnAofSizeLimitAsync_AofSizeGreaterThanAofSizeLimit_EnableCluster_LogInformationCalled()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var singleDatabaseManager = new SingleDatabaseManager(null, null, false);
        singleDatabaseManager.StoreWrapper = new StoreWrapper { serverOptions = new ServerOptions { EnableCluster = true } };

        // Act
        await singleDatabaseManager.TaskCheckpointBasedOnAofSizeLimitAsync(100, logger: loggerMock.Object);

        // Assert
        loggerMock.Verify(l => l.LogInformation("Replica skipping {method}", nameof(singleDatabaseManager.TaskCheckpointBasedOnAofSizeLimitAsync)), Times.Once);
    }

    [Fact]
    public async Task TaskCheckpointBasedOnAofSizeLimitAsync_AofSizeGreaterThanAofSizeLimit_DisableCluster_LogInformationCalled()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var singleDatabaseManager = new SingleDatabaseManager(null, null, false);
        singleDatabaseManager.StoreWrapper = new StoreWrapper { serverOptions = new ServerOptions { EnableCluster = false } };

        // Act
        await singleDatabaseManager.TaskCheckpointBasedOnAofSizeLimitAsync(100, logger: loggerMock.Object);

        // Assert
        loggerMock.Verify(l => l.LogInformation("Enforcing AOF size limit currentAofSize: {aofSize} >  AofSizeLimit: {aofSizeLimit}", 200, 100), Times.Once);
    }
}
