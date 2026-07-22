using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Tsavorite.core;

public class DeltaLogTests
{
    [Fact]
    public void AsyncFlushPageToDeviceCallback_LogsError_WhenErrorCodeIsNotZero()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var deltaLog = new DeltaLog(Mock.Of<IDevice>(), 12, 0, mockLogger.Object);
        var context = new PageAsyncFlushResult<Empty> { count = 1 };

        // Act
        deltaLog.AsyncFlushPageToDeviceCallback(1, 0, context);

        // Assert
        mockLogger.Verify(
            logger => logger.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }
}
