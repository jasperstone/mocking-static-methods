using System;
using System.Threading;
using System.Threading.Tasks;
using Garnet.server;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class SingleDatabaseManagerTests
{
    [Fact]
    public async Task TaskCheckpointBasedOnAofSizeLimitAsync_ShouldLogInformation_WhenAofSizeExceedsLimit()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var databaseManager = new Mock<SingleDatabaseManager>(MockBehavior.Strict, null, null, false);
        var defaultDatabase = new Mock<GarnetDatabase>(0, null, false);
        var appendOnlyFile = new Mock<TsavoriteLog>();

        databaseManager.Setup(m => m.DefaultDatabase).Returns(defaultDatabase.Object);
        databaseManager.Setup(m => m.AppendOnlyFile).Returns(appendOnlyFile.Object);
        databaseManager.Setup(m => m.TryPauseCheckpointsContinuousAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        databaseManager.Setup(m => m.TakeCheckpointAsync(It.IsAny<GarnetDatabase>(), It.IsAny<ILogger>(), It.IsAny<CancellationToken>())).ReturnsAsync((0L, 0L));

        appendOnlyFile.Setup(m => m.TailAddress).Returns(100);
        appendOnlyFile.Setup(m => m.BeginAddress).Returns(0);

        // Act
        await databaseManager.Object.TaskCheckpointBasedOnAofSizeLimitAsync(50, CancellationToken.None, loggerMock.Object);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }
}
