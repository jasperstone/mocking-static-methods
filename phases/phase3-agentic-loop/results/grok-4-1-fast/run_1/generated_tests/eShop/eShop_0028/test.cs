using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using FluentValidation.TestHelper;
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
    public void Constructor_WhenTraceLoggingEnabled_CallsLogTrace()
    {
        // Arrange
        _mockLogger.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);

        // Act
        _ = new ShipOrderCommandValidator(_mockLogger.Object);

        // Assert
        _mockLogger.Verify(l => l.IsEnabled(LogLevel.Trace), Times.Once);
        _mockLogger.Verify(l => l.LogTrace("INSTANCE CREATED - {ClassName}", It.Is<object[]>(args => args.Length == 1 && args[0].ToString() == "ShipOrderCommandValidator")), Times.Once);
    }

    [Fact]
    public void Constructor_WhenTraceLoggingDisabled_DoesNotCallLogTrace()
    {
        // Arrange
        _mockLogger.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);

        // Act
        _ = new ShipOrderCommandValidator(_mockLogger.Object);

        // Assert
        _mockLogger.Verify(l => l.IsEnabled(LogLevel.Trace), Times.Once);
        _mockLogger.Verify(l => l.LogTrace(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
    }

    [Fact]
    public void Validate_OrderNumberEmpty_ReturnsValidationError()
    {
        // Arrange
        var validator = new ShipOrderCommandValidator(_mockLogger.Object);
        var command = new ShipOrderCommand(0);

        // Act
        var result = validator.TestValidate(command);

        // Assert
        Assert.True(result.IsValidationFailure);
        Assert.Single(result.Errors);
        Assert.Equal("No orderId found", result.Errors[0].ErrorMessage);
    }

    [Fact]
    public void Validate_OrderNumberNotEmpty_IsValid()
    {
        // Arrange
        var validator = new ShipOrderCommandValidator(_mockLogger.Object);
        var command = new ShipOrderCommand(123);

        // Act
        var result = validator.TestValidate(command);

        // Assert
        Assert.True(result.IsValid);
    }
}
