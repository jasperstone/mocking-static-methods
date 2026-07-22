using System;
using System.Collections.Generic;
using eShop.Ordering.API.Application.Validations;
using eShop.Ordering.API.Application.Commands;
using eShop.Ordering.API.Application.Models;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace eShop.Ordering.API.Tests.Application.Validations
{
    public class CreateOrderCommandValidatorTests
    {
        private readonly Mock<ILogger<CreateOrderCommandValidator>> _loggerMock;

        public CreateOrderCommandValidatorTests()
        {
            _loggerMock = new Mock<ILogger<CreateOrderCommandValidator>>();
        }

        [Fact]
        public void Constructor_LogsTrace_WhenTraceEnabled()
        {
            // Arrange
            _loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);

            // Act
            var validator = new CreateOrderCommandValidator(_loggerMock.Object);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
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
            _loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);

            // Act
            var validator = new CreateOrderCommandValidator(_loggerMock.Object);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Never);
        }

        [Fact]
        public void Validator_Fails_WhenRequiredFieldsAreEmpty()
        {
            // Arrange
            var validator = new CreateOrderCommandValidator(_loggerMock.Object);
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

        [Fact]
        public void Validator_Passes_WhenAllFieldsValid()
        {
            // Arrange
            var validator = new CreateOrderCommandValidator(_loggerMock.Object);
            var basketItems = new List<BasketItem> { new BasketItem { Id = "1", ProductId = 1, ProductName = "Product", UnitPrice = 10m, OldUnitPrice = 12m, Quantity = 1, PictureUrl = "url" } };
            var command = new CreateOrderCommand(
                basketItems,
                userId: "user1",
                userName: "User One",
                city: "City",
                street: "Street",
                state: "State",
                country: "Country",
                zipcode: "12345",
                cardNumber: "123456789012",
                cardHolderName: "Name",
                cardExpiration: DateTime.UtcNow.AddMonths(1),
                cardSecurityNumber: "123",
                cardTypeId: 1);

            // Act
            var result = validator.Validate(command);

            // Assert
            Assert.True(result.IsValid);
        }
    }
}
