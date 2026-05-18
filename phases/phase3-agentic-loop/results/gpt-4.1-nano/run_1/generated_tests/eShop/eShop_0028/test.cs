using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using FluentValidation;
using eShop.Ordering.API.Application.Validations;

namespace eShop.Tests
{
    public class ShipOrderCommandValidatorTests
    {
        public class ShipOrderCommand
        {
            public string OrderNumber { get; set; }
        }

        [Fact]
        public void Constructor_Should_LogTrace_When_TraceEnabled()
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
        public void Constructor_Should_NotLog_When_TraceDisabled()
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
