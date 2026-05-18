using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;

public class LoggerExtensionsTests
{
    [Fact]
    public void IOCallback_LogsError_WhenErrorCodeIsNotZero()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        uint errorCode = 1;
        uint numBytes = 10;
        object context = new object();

        // Act
        LoggerExtensions.IOCallback(loggerMock.Object, errorCode, numBytes, context);

        // Assert
        loggerMock.Verify(
            logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("[ClusterUtils] OverlappedStream GetQueuedCompletionStatus error: 1 msg: ")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public void IOCallback_DoesNotLogError_WhenErrorCodeIsZero()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        uint errorCode = 0;
        uint numBytes = 10;
        object context = new object();

        // Act
        LoggerExtensions.IOCallback(loggerMock.Object, errorCode, numBytes, context);

        // Assert
        loggerMock.Verify(
            logger => logger.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Never);
    }
}
