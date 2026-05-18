using Xunit;
using Microsoft.Extensions.Logging;
using FluentValidation;
using Moq;
using eShop.Ordering.API.Application.Validations;

namespace eShop.Ordering.API.Tests.Application.Validations;

public class IdentifiedCommandValidatorTests
{
    [Fact]
    public void Constructor_WithTraceLoggingEnabled_LogsTraceMessage()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<IdentifiedCommandValidator>>();
        mockLogger.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);
        var logger = mockLogger.Object;

        // Act
        _ = new IdentifiedCommandValidator(logger);

        // Assert
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("INSTANCE CREATED - IdentifiedCommandValidator")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void Constructor_WithTraceLoggingDisabled_DoesNotLogTraceMessage()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<IdentifiedCommandValidator>>();
        mockLogger.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);
        var logger = mockLogger.Object;

        // Act
        _ = new IdentifiedCommandValidator(logger);

        // Assert
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }
}
