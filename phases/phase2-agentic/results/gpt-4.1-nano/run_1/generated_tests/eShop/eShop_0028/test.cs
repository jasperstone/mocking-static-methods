using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using FluentValidation;
using eShop.Ordering.API.Application.Validations;

namespace eShop.Ordering.Tests
{
    public class ShipOrderCommandValidatorTests
    {
        [Fact]
        public void Constructor_ShouldLogTrace_WhenTraceEnabled()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ShipOrderCommandValidator>>();
            mockLogger.Setup(x => x.IsEnabled(LogLevel.Trace)).Returns(true);
            var validator = new ShipOrderCommandValidator(mockLogger.Object);

            // Act
            // No action needed, constructor runs

            // Assert
            mockLogger.Verify(x => x.LogTrace("INSTANCE CREATED - {ClassName}", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void Constructor_ShouldNotLogTrace_WhenTraceDisabled()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ShipOrderCommandValidator>>();
            mockLogger.Setup(x => x.IsEnabled(LogLevel.Trace)).Returns(false);
            var validator = new ShipOrderCommandValidator(mockLogger.Object);

            // Act
            // No action needed, constructor runs

            // Assert
            mockLogger.Verify(x => x.LogTrace(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
        }
    }
}
