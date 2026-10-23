using System;
using System.Collections.Generic;
using System.Linq;
using eShop.Ordering.API.Application.Validations;
using eShop.Ordering.API.Application.Commands;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

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
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("INSTANCE CREATED")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void Constructor_DoesNotLogTrace_WhenTraceNotEnabled()
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
            var validator = new CreateOrderCommandValidator(loggerMock.Object);
            var command = new CreateOrderCommand();

            // Act
            var result = validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.City);
            result.ShouldHaveValidationErrorFor(c => c.Street);
            result.ShouldHaveValidationErrorFor(c => c.State);
            result.ShouldHaveValidationErrorFor(c => c.Country);
            result.ShouldHaveValidationErrorFor(c => c.ZipCode);
            result.ShouldHaveValidationErrorFor(c => c.CardNumber);
            result.ShouldHaveValidationErrorFor(c => c.CardHolderName);
            result.ShouldHaveValidationErrorFor(c => c.CardExpiration);
            result.ShouldHaveValidationErrorFor(c => c.CardSecurityNumber);
            result.ShouldHaveValidationErrorFor(c => c.CardTypeId);
            result.ShouldHaveValidationErrorFor(c => c.OrderItems);
        }

        [Fact]
        public void Validator_ValidatesCardExpirationDate()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CreateOrderCommandValidator>>();
            var validator = new CreateOrderCommandValidator(loggerMock.Object);
            var command = new CreateOrderCommand();

            // Act
            var result = validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.CardExpiration)
                .WithErrorMessage("Please specify a valid card expiration date");
        }
    }
}
