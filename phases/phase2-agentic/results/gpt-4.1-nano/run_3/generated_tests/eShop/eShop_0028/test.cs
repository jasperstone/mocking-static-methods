using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using FluentValidation;
using eShop.Ordering.API.Application.Validations;

namespace eShop.Ordering.Tests
{
    public class ShipOrderCommandValidatorTests
    {
        public class DummyShipOrderCommand
        {
            public string OrderNumber { get; set; }
        }

        [Fact]
        public void Constructor_ShouldLogTrace_WhenTraceEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ShipOrderCommandValidator>>();
            loggerMock.Setup(x => x.IsEnabled(LogLevel.Trace)).Returns(true);
            var called = false;
            loggerMock.Setup(x => x.LogTrace(It.IsAny<string>(), It.IsAny<object>()))
                .Callback<string, object>((msg, arg) => { called = true; });

            // Act
            var validator = new ShipOrderCommandValidator(loggerMock.Object);

            // Assert
            Assert.NotNull(validator);
            Assert.True(called);
            loggerMock.Verify(x => x.IsEnabled(LogLevel.Trace), Times.Once);
            loggerMock.Verify(x => x.LogTrace("INSTANCE CREATED - {ClassName}", It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public void Constructor_ShouldNotLogTrace_WhenTraceDisabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ShipOrderCommandValidator>>();
            loggerMock.Setup(x => x.IsEnabled(LogLevel.Trace)).Returns(false);

            // Act
            var validator = new ShipOrderCommandValidator(loggerMock.Object);

            // Assert
            Assert.NotNull(validator);
            loggerMock.Verify(x => x.LogTrace(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        }
    }
}
