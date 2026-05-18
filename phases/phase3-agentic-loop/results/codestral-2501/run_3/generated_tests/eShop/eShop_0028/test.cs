using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using eShop.Ordering.API.Application.Validations;
using FluentValidation;
using eShop.Ordering.API.Application.Commands;

namespace eShop.Ordering.API.Application.Tests.Validations
{
    public class ShipOrderCommandValidatorTests
    {
        [Fact]
        public void Constructor_ShouldLogTrace_WhenTraceIsEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ShipOrderCommandValidator>>();
            loggerMock.Setup(logger => logger.IsEnabled(LogLevel.Trace)).Returns(true);

            // Act
            var validator = new ShipOrderCommandValidator(loggerMock.Object);

            // Assert
            loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("INSTANCE CREATED - ShipOrderCommandValidator")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void Constructor_ShouldNotLogTrace_WhenTraceIsDisabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ShipOrderCommandValidator>>();
            loggerMock.Setup(logger => logger.IsEnabled(LogLevel.Trace)).Returns(false);

            // Act
            var validator = new ShipOrderCommandValidator(loggerMock.Object);

            // Assert
            loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("INSTANCE CREATED - ShipOrderCommandValidator")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Never);
        }

        [Fact]
        public void ShouldHaveError_WhenOrderNumberIsEmpty()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ShipOrderCommandValidator>>();
            var validator = new ShipOrderCommandValidator(loggerMock.Object);
            var command = new ShipOrderCommand(0);

            // Act
            var result = validator.Validate(command);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, error => error.ErrorMessage == "No orderId found");
        }
    }
}
