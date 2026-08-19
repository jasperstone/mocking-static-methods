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

        // Act
        var method = typeof(DeltaLog).GetMethod("AsyncFlushPageToDeviceCallback", BindingFlags.NonPublic | BindingFlags.Instance);
        method.Invoke(deltaLog, new object[] { 1u, 0u, new PageAsyncFlushResult<Empty>() });

        // Assert
        mockLogger.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("AsyncFlushPageToDeviceCallback error: 1")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }
}
