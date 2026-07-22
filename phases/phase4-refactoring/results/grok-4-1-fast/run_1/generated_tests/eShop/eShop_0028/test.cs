using FluentValidation.TestHelper;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using eShop.Ordering.API.Application.Validations;
using eShop.Ordering.API.Application.Commands;

namespace eShop.Ordering.API.Application.Tests.Validations;

public class ShipOrderCommandValidatorTests
{
    [Fact]
    public void Constructor_WhenTraceLoggingEnabled_CallsLogTrace()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ShipOrderCommandValidator>>();
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);

        // Act
        _ = new ShipOrderCommandValidator(loggerMock.Object);

        // Assert
        loggerMock.Verify(
            l => l.LogTrace(
                "INSTANCE CREATED - {ClassName}",
                It.Is<object[]>(args => args.Length == 1 && args[0].ToString() == "ShipOrderCommandValidator")),
            Times.Once);
    }

    [Fact]
    public void Constructor_WhenTraceLoggingDisabled_DoesNotCallLogTrace()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ShipOrderCommandValidator>>();
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);

        // Act
        _ = new ShipOrderCommandValidator(loggerMock.Object);

        // Assert
        loggerMock.Verify(l => l.LogTrace(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
    }

    [Fact]
    public void Validate_WhenOrderNumberEmpty_ReturnsValidationError()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ShipOrderCommandValidator>>();
        var validator = new ShipOrderCommandValidator(loggerMock.Object);
        var command = new ShipOrderCommand(0);

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.OrderNumber)
              .WithErrorMessage("No orderId found");
    }

    [Fact]
    public void Validate_WhenOrderNumberValid_NoValidationErrors()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ShipOrderCommandValidator>>();
        var validator = new ShipOrderCommandValidator(loggerMock.Object);
        var command = new ShipOrderCommand(123);

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.OrderNumber);
    }
}
