using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Tsavorite.core;

public class IndexRecoveryTests
{
    [Fact]
    public void AsyncPageReadCallback_LogsError_WhenErrorCodeIsNotZero()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TsavoriteBase>>();
        var indexRecovery = new TsavoriteBase
        {
            logger = mockLogger.Object
        };

        uint errorCode = 1;
        uint numBytes = 100;
        var overlap = new HashIndexPageAsyncReadResult { chunkIndex = 0 };

        // Act
        indexRecovery.AsyncPageReadCallback(errorCode, numBytes, overlap);

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

    [Fact]
    public void AsyncPageReadCallback_DoesNotLogError_WhenErrorCodeIsZero()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TsavoriteBase>>();
        var indexRecovery = new TsavoriteBase
        {
            logger = mockLogger.Object
        };

        uint errorCode = 0;
        uint numBytes = 100;
        var overlap = new HashIndexPageAsyncReadResult { chunkIndex = 0 };

        // Act
        indexRecovery.AsyncPageReadCallback(errorCode, numBytes, overlap);

        // Assert
        mockLogger.Verify(
            logger => logger.LogError(
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Never);
    }
}
