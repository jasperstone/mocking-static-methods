using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using FluentValidation;
using eShop.Ordering.API.Application.Validations;
using eShop.Ordering.API.Application.Commands;
using eShop.Ordering.API.Application.Models;

namespace eShop.Ordering.API.Application.Tests.Validations;

public class CreateOrderCommandValidatorTests
{
    [Fact]
    public void Constructor_WhenTraceLoggingEnabled_LogsTraceMessage()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<CreateOrderCommandValidator>>();
        mockLogger.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);

        // Act
        _ = new CreateOrderCommandValidator(mockLogger.Object);

        // Assert
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Trace,
                0,
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("INSTANCE CREATED - {ClassName}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void Constructor_WhenTraceLoggingDisabled_DoesNotLogTraceMessage()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<CreateOrderCommandValidator>>();
        mockLogger.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);

        // Act
        _ = new CreateOrderCommandValidator(mockLogger.Object);

        // Assert
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Trace,
                0,
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact]
    public void Validate_ValidCommand_ReturnsValid()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<CreateOrderCommandValidator>>();
        var validator = new CreateOrderCommandValidator(mockLogger.Object);

        var command = new CreateOrderCommand(
            new List<BasketItem> { new() { ProductId = 1, ProductName = "Test", UnitPrice = 10, Quantity = 1 } },
            "user1", "User Name", "City", "Street", "State", "Country", "12345",
            "1234567890123456", "Holder", DateTime.UtcNow.AddYears(1), "123", 1);

        // Act
        var result = validator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
    }
}
