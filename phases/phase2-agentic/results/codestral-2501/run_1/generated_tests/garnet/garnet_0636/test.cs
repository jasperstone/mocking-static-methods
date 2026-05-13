using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Tsavorite.core;

public class DeltaLogTests
{
    [Fact]
    public void AsyncFlushPageToDeviceCallback_LogsError_WhenErrorCodeIsNonZero()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var deltaLog = new DeltaLog(null, 0, 0, mockLogger.Object);

        uint errorCode = 1;
        uint numBytes = 0;
        var context = new PageAsyncFlushResult<Empty> { count = 1 };

        // Act
        deltaLog.AsyncFlushPageToDeviceCallback(errorCode, numBytes, context);

        // Assert
        mockLogger.Verify(
            logger => logger.LogError(
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }
}
