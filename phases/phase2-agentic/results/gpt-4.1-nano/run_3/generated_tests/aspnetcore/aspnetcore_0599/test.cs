using System;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;

namespace Microsoft.Extensions.Logging
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void LogError_ShouldCallLoggerLogError_WithExpectedParameters()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var exception = new InvalidOperationException("Test exception");
            var message = "Error occurred";

            // Act
            mockLogger.Object.LogError(exception, message);

            // Assert
            mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == message),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
