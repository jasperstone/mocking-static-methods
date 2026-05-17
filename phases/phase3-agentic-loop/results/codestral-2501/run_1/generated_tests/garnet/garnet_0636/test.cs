using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Tsavorite.core;
using System.Reflection;

public class DeltaLogTests
{
    [Fact]
    public void AsyncFlushPageToDeviceCallback_LogsError_WhenErrorCodeIsNotZero()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var deltaLog = new DeltaLog(Mock.Of<IDevice>(), 12, 0, mockLogger.Object);
        var errorCode = 1u;
        var context = new PageAsyncFlushResult<Empty> { count = 1 };

        // Act
        var method = typeof(DeltaLog).GetMethod("AsyncFlushPageToDeviceCallback", BindingFlags.NonPublic | BindingFlags.Instance);
        method.Invoke(deltaLog, new object[] { errorCode, 0u, context });

        // Assert
        mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
            Times.Once);
    }
}
