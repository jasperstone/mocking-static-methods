using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Tsavorite.core;
using System.Reflection;

public class IndexRecoveryTests
{
    private class TestableTsavoriteBase : TsavoriteBase
    {
        public new ILogger<TsavoriteBase> logger { get; set; }

        public void InvokeAsyncPageReadCallback(uint errorCode, uint numBytes, object overlap)
        {
            AsyncPageReadCallback(errorCode, numBytes, overlap);
        }
    }

    [Fact]
    public void AsyncPageReadCallback_LogsError_WhenErrorCodeIsNotZero()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TsavoriteBase>>();
        var indexRecovery = new TestableTsavoriteBase
        {
            logger = mockLogger.Object
        };

        uint errorCode = 1;
        uint numBytes = 100;
        var overlap = new object();

        // Act
        indexRecovery.InvokeAsyncPageReadCallback(errorCode, numBytes, overlap);

        // Assert
        mockLogger.Verify(
            logger => logger.LogError(
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
