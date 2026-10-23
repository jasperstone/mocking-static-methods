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
        mockLogger.Verify(l => l.LogTrace("INSTANCE CREATED - {ClassName}", It.IsAny<object>()), Times.Once);
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
    public void Constructor_CreatesValidatorSuccessfully()
    {
        // Arrange
        var logger = NullLogger<IdentifiedCommandValidator>.Instance;

        // Act
        var validator = new IdentifiedCommandValidator(logger);

        // Assert
        Assert.NotNull(validator);
    }
}
