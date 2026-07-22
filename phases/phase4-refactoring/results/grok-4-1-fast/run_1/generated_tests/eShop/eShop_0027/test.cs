using FluentValidation.TestHelper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;
using eShop.Ordering.API.Application.Validations;
using eShop.Ordering.API.Application.Commands;

namespace eShop.Ordering.API.Application.Tests.Validations;

public class IdentifiedCommandValidatorTests
{
    [Fact]
    public void Constructor_WithTraceEnabled_LogsTraceMessage()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<IdentifiedCommandValidator>>();
        mockLogger.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);

        // Act
        _ = new IdentifiedCommandValidator(mockLogger.Object);

        // Assert
        mockLogger.Verify(l => l.LogTrace("INSTANCE CREATED - {ClassName}", "IdentifiedCommandValidator"), Times.Once);
    }

    [Fact]
    public void Constructor_WithTraceDisabled_DoesNotLogTraceMessage()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<IdentifiedCommandValidator>>();
        mockLogger.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);

        // Act
        _ = new IdentifiedCommandValidator(mockLogger.Object);

        // Assert
        mockLogger.Verify(l => l.LogTrace(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
    }

    [Fact]
    public void Constructor_SetsUpIdNotEmptyValidationRule()
    {
        // Arrange
        var logger = NullLogger<IdentifiedCommandValidator>.Instance;
        var validator = new IdentifiedCommandValidator(logger);

        // Act & Assert - test with empty Id
        var command = new IdentifiedCommand<CreateOrderCommand, bool> { Id = "" };
        var result = validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public void Constructor_WithValidId_ValidatesSuccessfully()
    {
        // Arrange
        var logger = NullLogger<IdentifiedCommandValidator>.Instance;
        var validator = new IdentifiedCommandValidator(logger);

        // Act & Assert - test with valid Id
        var command = new IdentifiedCommand<CreateOrderCommand, bool> { Id = "valid-id" };
        var result = validator.TestValidate(command);
        result.IsValid.Should().BeTrue();
    }
}
