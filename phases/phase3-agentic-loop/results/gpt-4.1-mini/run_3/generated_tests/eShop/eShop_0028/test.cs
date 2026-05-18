using System;
using eShop.Ordering.API.Application.Validations;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace eShop.Ordering.API.Tests.Application.Validations
{
    public class ShipOrderCommandValidatorTests
    {
        private class TestShipOrderCommand
        {
            public string OrderNumber { get; set; }
        }

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
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("INSTANCE CREATED")),
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
            var validator = new ShipOrderCommandValidator(loggerMock.Object);

            var invalidCommand = new TestShipOrderCommand { OrderNumber = "" };
            var validCommand = new TestShipOrderCommand { OrderNumber = "123" };

            // Act
            var invalidResult = validator.Validate(invalidCommand);
            var validResult = validator.Validate(validCommand);

            // Assert
            Assert.False(invalidResult.IsValid);
            Assert.Contains(invalidResult.Errors, e => e.PropertyName == "OrderNumber" && e.ErrorMessage == "No orderId found");
            Assert.True(validResult.IsValid);
        }
    }
}
