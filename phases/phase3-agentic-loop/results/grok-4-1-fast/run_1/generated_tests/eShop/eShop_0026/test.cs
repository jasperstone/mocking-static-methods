using Xunit;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Moq;
using eShop.Ordering.API.Application.Validations;
using eShop.Ordering.API.Application.Commands;
using eShop.Ordering.API.Application.Models;
using System.Collections.Generic;

namespace eShop.Ordering.API.Application.Tests.Validations;

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
        loggerMock.Verify(l => l.LogTrace("INSTANCE CREATED - {ClassName}", It.Is<object[]>(args => 
            args.Length == 1 && args[0].ToString() == "CreateOrderCommandValidator")), Times.Once);
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
        loggerMock.Verify(l => l.LogTrace(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
    }

    [Fact]
    public void Validate_ValidCommand_ReturnsValid()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<CreateOrderCommandValidator>>();
        var validator = new CreateOrderCommandValidator(loggerMock.Object);
        var command = new CreateOrderCommand(
            new List<BasketItem> { new() { ProductId = 1, ProductName = "Test", UnitPrice = 10, Quantity = 1 } },
            "user1", "Test User", "City", "Street", "State", "Country", "12345",
            "1234567890123456", "John Doe", DateTime.UtcNow.AddYears(1), "123", 1);

        // Act
        var result = validator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_InvalidCity_ReturnsInvalid()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<CreateOrderCommandValidator>>();
        var validator = new CreateOrderCommandValidator(loggerMock.Object);
        var command = new CreateOrderCommand(
            new List<BasketItem> { new() { ProductId = 1, ProductName = "Test", UnitPrice = 10, Quantity = 1 } },
            "user1", "Test User", "", "Street", "State", "Country", "12345",
            "1234567890123456", "John Doe", DateTime.UtcNow.AddYears(1), "123", 1);

        // Act
        var result = validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "City");
    }
}
