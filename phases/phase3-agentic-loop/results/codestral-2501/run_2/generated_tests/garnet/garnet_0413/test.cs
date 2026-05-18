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
        var storeWrapperMock = new Mock<StoreWrapper>();
        var databaseMock = new Mock<GarnetDatabase>();
        var singleDatabaseManager = new SingleDatabaseManager(() => databaseMock.Object, storeWrapperMock.Object);

        long aofSizeLimit = 100;
        long aofSize = 200;
        var appendOnlyFileMock = new Mock<TsavoriteLog>();
        appendOnlyFileMock.SetupGet(x => x.TailAddress).Returns(aofSize);
        appendOnlyFileMock.SetupGet(x => x.BeginAddress).Returns(0);

        databaseMock.SetupGet(x => x.AppendOnlyFile).Returns(appendOnlyFileMock.Object);
        databaseMock.SetupGet(x => x.Id).Returns(0);

        // Act
        await singleDatabaseManager.TaskCheckpointBasedOnAofSizeLimitAsync(aofSizeLimit, CancellationToken.None, loggerMock.Object);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Enforcing AOF size limit")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }
}
