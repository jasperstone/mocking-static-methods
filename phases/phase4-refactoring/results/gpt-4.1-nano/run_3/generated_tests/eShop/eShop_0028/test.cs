using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using eShop.Ordering.API.Application.Validations;

namespace eShop.Tests
{
    public class ShipOrderCommandValidatorTests
    {
        [Fact]
        public void Constructor_ShouldLogTrace_WhenTraceEnabled()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ShipOrderCommandValidator>>();
            mockLogger.Setup(x => x.IsEnabled(LogLevel.Trace)).Returns(true);
            mockLogger.Setup(x => x.LogTrace(It.IsAny<string>(), It.IsAny<object>()))
                      .Verifiable();

            // Act
            var validator = new ShipOrderCommandValidator(mockLogger.Object);

            // Assert
            mockLogger.Verify(x => x.LogTrace("INSTANCE CREATED - {ClassName}", "ShipOrderCommandValidator"), Times.Once);
        }

        [Fact]
        public void Constructor_ShouldNotLogTrace_WhenTraceDisabled()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ShipOrderCommandValidator>>();
            mockLogger.Setup(x => x.IsEnabled(LogLevel.Trace)).Returns(false);
            mockLogger.Setup(x => x.LogTrace(It.IsAny<string>(), It.IsAny<object>()))
                      .Verifiable();

            // Act
            var validator = new ShipOrderCommandValidator(mockLogger.Object);

            // Assert
            mockLogger.Verify(x => x.LogTrace(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        }
    }
}
