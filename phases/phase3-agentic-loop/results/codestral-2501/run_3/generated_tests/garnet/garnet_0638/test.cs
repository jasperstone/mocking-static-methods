using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Tsavorite.core;

public class IndexRecoveryTests
{
    [Fact]
    public void AsyncPageReadCallback_LogsError_WhenErrorCodeIsNotZero()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var indexRecovery = new TsavoriteBase();
        indexRecovery.logger = mockLogger.Object;
        var overlap = new object(); // Since HashIndexPageAsyncReadResult is not available
        uint errorCode = 1;
        uint numBytes = 1024;

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
}
