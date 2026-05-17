using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using eShop.Ordering.API.Application.Validations;
using eShop.Ordering.API.Application.Commands;
using FluentValidation;

namespace eShop.Ordering.API.Tests
{
    public class ShipOrderCommandValidatorTests
    {
        [Fact]
        public void Constructor_LogsTrace_WhenLogLevelIsTrace()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ShipOrderCommandValidator>>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);
            var logger = loggerMock.Object;

            // Act
            var validator = new ShipOrderCommandValidator(logger);

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString().Contains("INSTANCE CREATED - ShipOrderCommandValidator")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
                ), Times.Once);
        }

        [Fact]
        public void Constructor_DoesNotLogTrace_WhenLogLevelIsNotTrace()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ShipOrderCommandValidator>>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);
            var logger = loggerMock.Object;

            // Act
            new ShipOrderCommandValidator(logger);

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
                ), Times.Never);
        }

        [Fact]
        public void Validate_OrderNumberIsEmpty_ReturnsError()
        {
            // Arrange
            var validator = new ShipOrderCommandValidator(new Mock<ILogger<ShipOrderCommandValidator>>().Object);
            var command = new ShipOrderCommand(0);

            // Act
            var result = validator.Validate(command);

            // Assert
            Assert.False(result.IsValid);
            Assert.Single(result.Errors);
            Assert.Equal("No orderId found", result.Errors[0].ErrorMessage);
        }

        [Fact]
        public void Validate_OrderNumberIsNotEmpty_ReturnsNoError()
        {
            // Arrange
            var validator = new ShipOrderCommandValidator(new Mock<ILogger<ShipOrderCommandValidator>>().Object);
            var command = new ShipOrderCommand(1);

            // Act
            var result = validator.Validate(command);

            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }
    }
}
