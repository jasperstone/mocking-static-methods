using System;
using System.Collections.Generic;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using eShop.Ordering.API.Application.Validations;
using eShop.Ordering.API.Application.Commands;

namespace eShop.Ordering.API.Tests.Application.Validations
{
    public class CreateOrderCommandValidatorTests
    {
        [Fact]
        public void Constructor_LogsTrace_WhenTraceEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CreateOrderCommandValidator>>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);

            // Act
            var validator = new CreateOrderCommandValidator(loggerMock.Object);

            // Assert
            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("INSTANCE CREATED - CreateOrderCommandValidator")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void Constructor_DoesNotLogTrace_WhenTraceDisabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CreateOrderCommandValidator>>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);

            // Act
            var validator = new CreateOrderCommandValidator(loggerMock.Object);

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
        public void Validator_ValidatesRequiredFields()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CreateOrderCommandValidator>>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);
            var validator = new CreateOrderCommandValidator(loggerMock.Object);

            var command = new CreateOrderCommand();

            // Act
            var result = validator.Validate(command);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "City");
            Assert.Contains(result.Errors, e => e.PropertyName == "Street");
            Assert.Contains(result.Errors, e => e.PropertyName == "State");
            Assert.Contains(result.Errors, e => e.PropertyName == "Country");
            Assert.Contains(result.Errors, e => e.PropertyName == "ZipCode");
            Assert.Contains(result.Errors, e => e.PropertyName == "CardNumber");
            Assert.Contains(result.Errors, e => e.PropertyName == "CardHolderName");
            Assert.Contains(result.Errors, e => e.PropertyName == "CardExpiration");
            Assert.Contains(result.Errors, e => e.PropertyName == "CardSecurityNumber");
            Assert.Contains(result.Errors, e => e.PropertyName == "CardTypeId");
            Assert.Contains(result.Errors, e => e.PropertyName == "OrderItems");
        }
    }
}
