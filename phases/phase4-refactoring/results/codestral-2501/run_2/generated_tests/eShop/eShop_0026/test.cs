using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using eShop.Ordering.API.Application.Validations;
using FluentValidation.TestHelper;
using eShop.Ordering.API.Application.Commands;
using System.Collections.Generic;

namespace eShop.Ordering.API.Application.Validations.Tests
{
    public class CreateOrderCommandValidatorTests
    {
        [Fact]
        public void LogTrace_ShouldBeCalled_WhenLoggerIsEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CreateOrderCommandValidator>>();
            loggerMock.Setup(logger => logger.IsEnabled(LogLevel.Trace)).Returns(true);

            // Act
            var validator = new CreateOrderCommandValidator(loggerMock.Object);

            // Assert
            loggerMock.Verify(logger => logger.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("INSTANCE CREATED - CreateOrderCommandValidator")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }

        [Fact]
        public void Should_Have_Error_When_City_Is_Empty()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CreateOrderCommandValidator>>();
            var validator = new CreateOrderCommandValidator(loggerMock.Object);
            var command = new CreateOrderCommand(new List<BasketItem>(), "userId", "userName", "", "street", "state", "country", "zipcode", "cardNumber", "cardHolderName", DateTime.UtcNow, "cardSecurityNumber", 1);

            // Act
            var result = validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.City);
        }

        [Fact]
        public void Should_Have_Error_When_Street_Is_Empty()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CreateOrderCommandValidator>>();
            var validator = new CreateOrderCommandValidator(loggerMock.Object);
            var command = new CreateOrderCommand(new List<BasketItem>(), "userId", "userName", "city", "", "state", "country", "zipcode", "cardNumber", "cardHolderName", DateTime.UtcNow, "cardSecurityNumber", 1);

            // Act
            var result = validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Street);
        }

        [Fact]
        public void Should_Have_Error_When_State_Is_Empty()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CreateOrderCommandValidator>>();
            var validator = new CreateOrderCommandValidator(loggerMock.Object);
            var command = new CreateOrderCommand(new List<BasketItem>(), "userId", "userName", "city", "street", "", "country", "zipcode", "cardNumber", "cardHolderName", DateTime.UtcNow, "cardSecurityNumber", 1);

            // Act
            var result = validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.State);
        }

        [Fact]
        public void Should_Have_Error_When_Country_Is_Empty()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CreateOrderCommandValidator>>();
            var validator = new CreateOrderCommandValidator(loggerMock.Object);
            var command = new CreateOrderCommand(new List<BasketItem>(), "userId", "userName", "city", "street", "state", "", "zipcode", "cardNumber", "cardHolderName", DateTime.UtcNow, "cardSecurityNumber", 1);

            // Act
            var result = validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Country);
        }

        [Fact]
        public void Should_Have_Error_When_ZipCode_Is_Empty()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CreateOrderCommandValidator>>();
            var validator = new CreateOrderCommandValidator(loggerMock.Object);
            var command = new CreateOrderCommand(new List<BasketItem>(), "userId", "userName", "city", "street", "state", "country", "", "cardNumber", "cardHolderName", DateTime.UtcNow, "cardSecurityNumber", 1);

            // Act
            var result = validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.ZipCode);
        }

        [Fact]
        public void Should_Have_Error_When_CardNumber_Is_Empty()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CreateOrderCommandValidator>>();
            var validator = new CreateOrderCommandValidator(loggerMock.Object);
            var command = new CreateOrderCommand(new List<BasketItem>(), "userId", "userName", "city", "street", "state", "country", "zipcode", "", "cardHolderName", DateTime.UtcNow, "cardSecurityNumber", 1);

            // Act
            var result = validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.CardNumber);
        }

        [Fact]
        public void Should_Have_Error_When_CardNumber_Is_Too_Short()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CreateOrderCommandValidator>>();
            var validator = new CreateOrderCommandValidator(loggerMock.Object);
            var command = new CreateOrderCommand(new List<BasketItem>(), "userId", "userName", "city", "street", "state", "country", "zipcode", "12345678901", "cardHolderName", DateTime.UtcNow, "cardSecurityNumber", 1);

            // Act
            var result = validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.CardNumber);
        }

        [Fact]
        public void Should_Have_Error_When_CardNumber_Is_Too_Long()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CreateOrderCommandValidator>>();
            var validator = new CreateOrderCommandValidator(loggerMock.Object);
            var command = new CreateOrderCommand(new List<BasketItem>(), "userId", "userName", "city", "street", "state", "country", "zipcode", "1234567890123456789", "cardHolderName", DateTime.UtcNow, "cardSecurityNumber", 1);

            // Act
            var result = validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.CardNumber);
        }

        [Fact]
        public void Should_Have_Error_When_CardHolderName_Is_Empty()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CreateOrderCommandValidator>>();
            var validator = new CreateOrderCommandValidator(loggerMock.Object);
            var command = new CreateOrderCommand(new List<BasketItem>(), "userId", "userName", "city", "street", "state", "country", "zipcode", "cardNumber", "", DateTime.UtcNow, "cardSecurityNumber", 1);

            // Act
            var result = validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.CardHolderName);
        }

        [Fact]
        public void Should_Have_Error_When_CardExpiration_Is_Empty()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CreateOrderCommandValidator>>();
            var validator = new CreateOrderCommandValidator(loggerMock.Object);
            var command = new CreateOrderCommand(new List<BasketItem>(), "userId", "userName", "city", "street", "state", "country", "zipcode", "cardNumber", "cardHolderName", DateTime.MinValue, "cardSecurityNumber", 1);

            // Act
            var result = validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.CardExpiration);
        }

        [Fact]
        public void Should_Have_Error_When_CardExpiration_Is_Invalid()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CreateOrderCommandValidator>>();
            var validator = new CreateOrderCommandValidator(loggerMock.Object);
            var command = new CreateOrderCommand(new List<BasketItem>(), "userId", "userName", "city", "street", "state", "country", "zipcode", "cardNumber", "cardHolderName", DateTime.UtcNow.AddDays(-1), "cardSecurityNumber", 1);

            // Act
            var result = validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.CardExpiration);
        }

        [Fact]
        public void Should_Have_Error_When_CardSecurityNumber_Is_Empty()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CreateOrderCommandValidator>>();
            var validator = new CreateOrderCommandValidator(loggerMock.Object);
            var command = new CreateOrderCommand(new List<BasketItem>(), "userId", "userName", "city", "street", "state", "country", "zipcode", "cardNumber", "cardHolderName", DateTime.UtcNow, "", 1);

            // Act
            var result = validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.CardSecurityNumber);
        }

        [Fact]
        public void Should_Have_Error_When_CardSecurityNumber_Is_Too_Short()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CreateOrderCommandValidator>>();
            var validator = new CreateOrderCommandValidator(loggerMock.Object);
            var command = new CreateOrderCommand(new List<BasketItem>(), "userId", "userName", "city", "street", "state", "country", "zipcode", "cardNumber", "cardHolderName", DateTime.UtcNow, "12", 1);

            // Act
            var result = validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.CardSecurityNumber);
        }

        [Fact]
        public void Should_Have_Error_When_CardSecurityNumber_Is_Too_Long()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CreateOrderCommandValidator>>();
            var validator = new CreateOrderCommandValidator(loggerMock.Object);
            var command = new CreateOrderCommand(new List<BasketItem>(), "userId", "userName", "city", "street", "state", "country", "zipcode", "cardNumber", "cardHolderName", DateTime.UtcNow, "1234", 1);

            // Act
            var result = validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.CardSecurityNumber);
        }

        [Fact]
        public void Should_Have_Error_When_CardTypeId_Is_Empty()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CreateOrderCommandValidator>>();
            var validator = new CreateOrderCommandValidator(loggerMock.Object);
            var command = new CreateOrderCommand(new List<BasketItem>(), "userId", "userName", "city", "street", "state", "country", "zipcode", "cardNumber", "cardHolderName", DateTime.UtcNow, "cardSecurityNumber", 0);

            // Act
            var result = validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.CardTypeId);
        }
    }
}
