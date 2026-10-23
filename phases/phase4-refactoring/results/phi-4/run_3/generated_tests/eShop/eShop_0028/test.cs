using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using eShop.Ordering.API.Application.Validations;

namespace eShop.Ordering.API.Tests.Application.Validations
{
    public class ShipOrderCommandValidatorTests
    {
        [Fact]
        public void Constructor_LogsTraceMessage_WhenLogLevelIsTrace()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ShipOrderCommandValidator>>();
            mockLogger.Setup(x => x.IsEnabled(LogLevel.Trace)).Returns(true);

            // Act
            var validator = new ShipOrderCommandValidator(mockLogger.Object);

            // Assert
            mockLogger.Verify(
                x => x.LogTrace("INSTANCE CREATED - {ClassName}", It.IsAny<string>()),
                Times.Once);
        }

        [Fact]
        public void Constructor_DoesNotLogTraceMessage_WhenLogLevelIsNotTrace()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ShipOrderCommandValidator>>();
            mockLogger.Setup(x => x.IsEnabled(LogLevel.Trace)).Returns(false);

            // Act
            var validator = new ShipOrderCommandValidator(mockLogger.Object);

            // Assert
            mockLogger.Verify(
                x => x.LogTrace("INSTANCE CREATED - {ClassName}", It.IsAny<string>()),
                Times.Never);
        }
    }
}
