using FluentValidation;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace eShop.Ordering.API.Application.Validations;

public class IdentifiedCommandValidatorTests
{
    [Fact]
    public void Constructor_WhenTraceLoggingEnabled_CallsLogTrace()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<IdentifiedCommandValidator>>();
        mockLogger.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);

        // Act
        _ = new IdentifiedCommandValidator(mockLogger.Object);

        // Assert
        mockLogger.Verify(l => l.LogTrace("INSTANCE CREATED - {ClassName}", It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    public void Constructor_WhenTraceLoggingDisabled_DoesNotCallLogTrace()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<IdentifiedCommandValidator>>();
        mockLogger.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);

        // Act
        _ = new IdentifiedCommandValidator(mockLogger.Object);

        // Assert
        mockLogger.Verify(l => l.LogTrace(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
    }
}
