using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using FluentValidation;
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
            var logTraceCalled = false;
            mockLogger.Setup(x => x.LogTrace(It.IsAny<string>(), It.IsAny<object>()))
                      .Callback<string, object>((msg, arg) => { logTraceCalled = true; });

            // Act
            var validator = new ShipOrderCommandValidator(mockLogger.Object);

            // Assert
            Assert.NotNull(validator);
            Assert.True(logTraceCalled, "LogTrace was not called when LogLevel.Trace is enabled");
        }

        [Fact]
        public void Constructor_ShouldNotLogTrace_WhenTraceDisabled()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ShipOrderCommandValidator>>();
            mockLogger.Setup(x => x.IsEnabled(LogLevel.Trace)).Returns(false);

            // Act
            var validator = new ShipOrderCommandValidator(mockLogger.Object);

            // Assert
            Assert.NotNull(validator);
            mockLogger.Verify(x => x.LogTrace(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        }
    }
}
