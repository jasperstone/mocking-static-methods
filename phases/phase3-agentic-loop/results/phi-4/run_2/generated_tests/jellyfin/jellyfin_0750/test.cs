using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Tests
{
    public class EncoderValidatorTests
    {
        [Fact]
        public void CheckFilterWithOption_LogsWarning_WhenFilterOrOptionNotAvailable()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<EncoderValidator>>();
            var encoderValidator = new EncoderValidator(mockLogger.Object);

            // Act
            encoderValidator.CheckFilterWithOption("nonexistent_filter", "option");

            // Assert
            mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Filter: nonexistent_filter with option option is not available")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
