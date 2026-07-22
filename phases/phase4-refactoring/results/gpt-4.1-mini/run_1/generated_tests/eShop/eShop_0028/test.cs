using System;
using eShop.Ordering.API.Application.Validations;
using eShop.Ordering.API.Application.Commands;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace eShop.Ordering.API.Application.Tests.Validations
{
    public class ShipOrderCommandValidatorTests
    {
        [Fact]
        public void Constructor_LogsTrace_WhenTraceEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ShipOrderCommandValidator>>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);

            // Act
            var validator = new ShipOrderCommandValidator(loggerMock.Object);

            // Assert
            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("INSTANCE CREATED - ShipOrderCommandValidator")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void Constructor_DoesNotLogTrace_WhenTraceNotEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ShipOrderCommandValidator>>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);

            // Act
            var validator = new ShipOrderCommandValidator(loggerMock.Object);

            // Assert
            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Never);
        }

        [Fact]
        public void Validate_OrderNumberZero_HasValidationError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ShipOrderCommandValidator>>();
            var validator = new ShipOrderCommandValidator(loggerMock.Object);
            var command = new ShipOrderCommand(0);

            // Act
            var result = validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.OrderNumber)
                .WithErrorMessage("No orderId found");
        }

        [Fact]
        public void Validate_OrderNumberNonZero_NoValidationError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ShipOrderCommandValidator>>();
            var validator = new ShipOrderCommandValidator(loggerMock.Object);
            var command = new ShipOrderCommand(123);

            // Act
            var result = validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(c => c.OrderNumber);
        }
    }
}
