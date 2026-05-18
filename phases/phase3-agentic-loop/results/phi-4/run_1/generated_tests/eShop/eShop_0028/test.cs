using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using eShop.Ordering.API.Application.Validations;
using FluentValidation;

namespace eShop.Ordering.API.Tests
{
    public class ShipOrderCommandValidatorTests
    {
        [Fact]
        public void Constructor_LogsTraceMessage_WhenLoggerIsEnabledForTrace()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ShipOrderCommandValidator>>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);
            var logTraceMock = new Mock<ILogger<ShipOrderCommandValidator>>();
            logTraceMock.Setup(l => l.LogTrace(It.IsAny<string>(), It.IsAny<object[]>()))
                        .Callback<string, object[]>((message, args) => 
                        {
                            Assert.Equal("INSTANCE CREATED - {ClassName}", message);
                            Assert.Equal(new[] { typeof(ShipOrderCommandValidator).Name }, args);
                        });

            // Act
            var validator = new ShipOrderCommandValidator(logTraceMock.Object);

            // Assert
            logTraceMock.Verify(l => l.LogTrace("INSTANCE CREATED - {ClassName}", new object[] { typeof(ShipOrderCommandValidator).Name }), Times.Once);
        }

        [Fact]
        public void Constructor_DoesNotLogTraceMessage_WhenLoggerIsNotEnabledForTrace()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ShipOrderCommandValidator>>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);
            var logTraceMock = new Mock<ILogger<ShipOrderCommandValidator>>();

            // Act
            var validator = new ShipOrderCommandValidator(logTraceMock.Object);

            // Assert
            logTraceMock.Verify(l => l.LogTrace(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
        }
    }
}
