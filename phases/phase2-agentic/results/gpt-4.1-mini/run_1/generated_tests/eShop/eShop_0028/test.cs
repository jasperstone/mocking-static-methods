using System;
using eShop.Ordering.API.Application.Validations;
using eShop.Ordering.API.Application.Commands;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace eShop.Ordering.API.Tests.Application.Validations
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
        public void RuleFor_OrderNumber_IsNotEmpty()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ShipOrderCommandValidator>>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);
            var validator = new ShipOrderCommandValidator(loggerMock.Object);

            // Act
            var result = validator.Validate(new ShipOrderCommand(0));

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "OrderNumber" && e.ErrorMessage == "No orderId found");
        }

        [Fact]
        public void RuleFor_OrderNumber_Passes_WhenNotEmpty()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ShipOrderCommandValidator>>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);
            var validator = new ShipOrderCommandValidator(loggerMock.Object);

            // Act
            var result = validator.Validate(new ShipOrderCommand(123));

            // Assert
            Assert.True(result.IsValid);
        }
    }
}
