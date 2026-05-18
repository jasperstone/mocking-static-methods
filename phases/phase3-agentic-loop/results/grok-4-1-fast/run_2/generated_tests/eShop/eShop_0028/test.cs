using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using FluentValidation;
using eShop.Ordering.API.Application.Validations;
using eShop.Ordering.API.Application.Commands;

namespace eShop.Ordering.API.Application.Tests.Validations;

public class ShipOrderCommandValidatorTests
{
    private readonly Mock<ILogger<ShipOrderCommandValidator>> _mockLogger;

    public ShipOrderCommandValidatorTests()
    {
        _mockLogger = new Mock<ILogger<ShipOrderCommandValidator>>();
    }

    [Fact]
    public void Constructor_WhenTraceEnabled_LogsTraceMessage()
    {
        // Arrange
        _mockLogger.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);

        // Act
        _ = new ShipOrderCommandValidator(_mockLogger.Object);

        // Assert
        _mockLogger.Verify(
            l => l.LogTrace(
                "INSTANCE CREATED - {ClassName}",
                It.Is<object[]>(args => args.Length == 1 && args[0].ToString() == "ShipOrderCommandValidator")
            ),
            Times.Once
        );
    }

    [Fact]
    public void Constructor_WhenTraceDisabled_DoesNotLogTraceMessage()
    {
        // Arrange
        _mockLogger.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);

        // Act
        _ = new ShipOrderCommandValidator(_mockLogger.Object);

        // Assert
        _mockLogger.Verify(
            l => l.LogTrace(
                It.IsAny<string>(),
                It.IsAny<object[]>()
            ),
            Times.Never
        );
    }

    [Fact]
    public void Constructor_SetsValidationRuleForOrderNumber_NotEmpty()
    {
        // Arrange
        _mockLogger.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);
        var validator = new ShipOrderCommandValidator(_mockLogger.Object);

        // Act
        var result = validator.Validate(new ShipOrderCommand(0));

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "No orderId found");
    }

    [Fact]
    public void Constructor_SetsValidationRuleForOrderNumber_ValidWhenNotEmpty()
    {
        // Arrange
        _mockLogger.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);
        var validator = new ShipOrderCommandValidator(_mockLogger.Object);

        // Act
        var result = validator.Validate(new ShipOrderCommand(123));

        // Assert
        Assert.True(result.IsValid);
    }
}
