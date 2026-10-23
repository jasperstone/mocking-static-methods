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
            var called = false;
            mockLogger.Setup(x => x.LogTrace(It.IsAny<string>(), It.IsAny<object>()))
                      .Callback<string, object>((msg, arg) => { called = true; });

            // Act
            var validator = new ShipOrderCommandValidator(mockLogger.Object);

            // Assert
            Assert.NotNull(validator);
            Assert.True(called);
            mockLogger.Verify(x => x.IsEnabled(LogLevel.Trace), Times.Once);
            mockLogger.Verify(x => x.LogTrace("INSTANCE CREATED - {ClassName}", It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public void Constructor_ShouldNotLog_WhenTraceNotEnabled()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ShipOrderCommandValidator>>();
            mockLogger.Setup(x => x.IsEnabled(LogLevel.Trace)).Returns(false);

            // Act
            var validator = new ShipOrderCommandValidator(mockLogger.Object);

            // Assert
            Assert.NotNull(validator);
            mockLogger.Verify(x => x.LogTrace("INSTANCE CREATED - {ClassName}", It.IsAny<object>()), Times.Never);
        }
    }
}
