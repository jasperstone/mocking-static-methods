using FluentValidation.TestHelper;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using eShop.Ordering.API.Application.Validations;
using eShop.Ordering.API.Application.Commands;
using eShop.Ordering.API.Application.Models;

namespace eShop.Ordering.API.UnitTests.Application.Validations;

public class CreateOrderCommandValidatorTests
{
    [Fact]
    public void Constructor_WhenLoggerTraceEnabled_LogsTraceMessage()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<CreateOrderCommandValidator>>();
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);

        // Act
        _ = new CreateOrderCommandValidator(loggerMock.Object);

        // Assert
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("INSTANCE CREATED - CreateOrderCommandValidator")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void Constructor_WhenLoggerTraceDisabled_DoesNotLogTraceMessage()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<CreateOrderCommandValidator>>();
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);

        // Act
        _ = new CreateOrderCommandValidator(loggerMock.Object);

        // Assert
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact]
    public void Validate_ValidCommand_HasNoValidationErrors()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<CreateOrderCommandValidator>>();
        var validator = new CreateOrderCommandValidator(loggerMock.Object);
        var command = new CreateOrderCommand(
            new List<BasketItem> { new BasketItem { ProductId = 1, ProductName = "Test", UnitPrice = 10m, Quantity = 1 } },
            "user1", "User Name", "City", "Street", "State", "Country", "12345",
            "1234567890123456", "Card Holder", DateTime.UtcNow.AddYears(1),
            "123", 1);

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_InvalidCity_HasValidationError()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<CreateOrderCommandValidator>>();
        var validator = new CreateOrderCommandValidator(loggerMock.Object);
        var command = new CreateOrderCommand(
            new List<BasketItem> { new BasketItem { ProductId = 1, ProductName = "Test", UnitPrice = 10m, Quantity = 1 } },
            "user1", "User Name", "", "Street", "State", "Country", "12345",
            "1234567890123456", "Card Holder", DateTime.UtcNow.AddYears(1),
            "123", 1);

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.City);
    }

    [Fact]
    public void Validate_ExpiredCard_HasValidationError()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<CreateOrderCommandValidator>>();
        var validator = new CreateOrderCommandValidator(loggerMock.Object);
        var command = new CreateOrderCommand(
            new List<BasketItem> { new BasketItem { ProductId = 1, ProductName = "Test", UnitPrice = 10m, Quantity = 1 } },
            "user1", "User Name", "City", "Street", "State", "Country", "12345",
            "1234567890123456", "Card Holder", DateTime.UtcNow.AddDays(-1),
            "123", 1);

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.CardExpiration)
              .WithErrorMessage("Please specify a valid card expiration date");
    }

    [Fact]
    public void Validate_EmptyOrderItems_HasValidationError()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<CreateOrderCommandValidator>>();
        var validator = new CreateOrderCommandValidator(loggerMock.Object);
        var command = new CreateOrderCommand(
            new List<BasketItem>(),
            "user1", "User Name", "City", "Street", "State", "Country", "12345",
            "1234567890123456", "Card Holder", DateTime.UtcNow.AddYears(1),
            "123", 1);

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.OrderItems)
              .WithErrorMessage("No order items found");
    }
}
