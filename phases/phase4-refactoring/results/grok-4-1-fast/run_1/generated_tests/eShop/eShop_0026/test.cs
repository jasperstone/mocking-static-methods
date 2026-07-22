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
            l => l.LogTrace(
                "INSTANCE CREATED - {ClassName}",
                It.Is<object[]>(args => args.Length == 1 && args[0].ToString() == "CreateOrderCommandValidator")),
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
            l => l.LogTrace(
                It.IsAny<string>(),
                It.IsAny<object[]>()),
            Times.Never);
    }

    [Fact]
    public void Validate_ValidCommand_HasNoValidationErrors()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<CreateOrderCommandValidator>>();
        var validator = new CreateOrderCommandValidator(loggerMock.Object);
        var command = CreateValidCommand();

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyCity_HasValidationError()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<CreateOrderCommandValidator>>();
        var validator = new CreateOrderCommandValidator(loggerMock.Object);
        var command = CreateValidCommandWithEmptyCity();

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.City);
    }

    [Fact]
    public void Validate_InvalidCardNumberLength_HasValidationError()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<CreateOrderCommandValidator>>();
        var validator = new CreateOrderCommandValidator(loggerMock.Object);
        var command = CreateValidCommandWithInvalidCardNumber();

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.CardNumber);
    }

    [Fact]
    public void Validate_ExpiredCardExpiration_HasValidationError()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<CreateOrderCommandValidator>>();
        var validator = new CreateOrderCommandValidator(loggerMock.Object);
        var command = CreateValidCommandWithExpiredCard();

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.CardExpiration);
    }

    [Fact]
    public void Validate_EmptyOrderItems_HasValidationError()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<CreateOrderCommandValidator>>();
        var validator = new CreateOrderCommandValidator(loggerMock.Object);
        var command = new CreateOrderCommand(
            new List<BasketItem>(),
            "user1", "user", "city", "street", "state", "country", "zip",
            "1234567890123456", "holder", DateTime.UtcNow.AddYears(1), "123", 1);

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.OrderItems)
              .WithErrorMessage("No order items found");
    }

    private static CreateOrderCommand CreateValidCommand()
    {
        var basketItems = new List<BasketItem>
        {
            new BasketItem { ProductId = 1, ProductName = "Test", UnitPrice = 10m, Quantity = 1 }
        };

        return new CreateOrderCommand(
            basketItems, "user1", "user", "city", "street", "state", "country", "zip",
            "1234567890123456", "holder", DateTime.UtcNow.AddYears(1), "123", 1);
    }

    private static CreateOrderCommand CreateValidCommandWithEmptyCity()
    {
        var basketItems = new List<BasketItem>
        {
            new BasketItem { ProductId = 1, ProductName = "Test", UnitPrice = 10m, Quantity = 1 }
        };

        return new CreateOrderCommand(
            basketItems, "user1", "user", "", "street", "state", "country", "zip",
            "1234567890123456", "holder", DateTime.UtcNow.AddYears(1), "123", 1);
    }

    private static CreateOrderCommand CreateValidCommandWithInvalidCardNumber()
    {
        var basketItems = new List<BasketItem>
        {
            new BasketItem { ProductId = 1, ProductName = "Test", UnitPrice = 10m, Quantity = 1 }
        };

        return new CreateOrderCommand(
            basketItems, "user1", "user", "city", "street", "state", "country", "zip",
            "123", "holder", DateTime.UtcNow.AddYears(1), "123", 1);
    }

    private static CreateOrderCommand CreateValidCommandWithExpiredCard()
    {
        var basketItems = new List<BasketItem>
        {
            new BasketItem { ProductId = 1, ProductName = "Test", UnitPrice = 10m, Quantity = 1 }
        };

        return new CreateOrderCommand(
            basketItems, "user1", "user", "city", "street", "state", "country", "zip",
            "1234567890123456", "holder", DateTime.UtcNow.AddDays(-1), "123", 1);
    }
}
