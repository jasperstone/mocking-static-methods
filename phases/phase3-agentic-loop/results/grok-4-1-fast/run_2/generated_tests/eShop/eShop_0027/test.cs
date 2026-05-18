using Xunit;
using Microsoft.Extensions.Logging;
using FluentValidation;
using eShop.Ordering.API.Application.Validations;
using Moq;
using System;

namespace eShop.Ordering.API.Application.Tests.Validations;

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
        mockLogger.Verify(
            l => l.LogTrace(
                It.Is<string>(msg => msg == "INSTANCE CREATED - {ClassName}"),
                It.IsAny<object[]>()),
            Times.Once);
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
        mockLogger.Verify(
            l => l.LogTrace(
                It.IsAny<string>(),
                It.IsAny<object[]>()),
            Times.Never);
    }

    [Fact]
    public void Constructor_CreatesValidatorSuccessfully()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<IdentifiedCommandValidator>>();

        // Act
        var validator = new IdentifiedCommandValidator(mockLogger.Object);

        // Assert
        Assert.NotNull(validator);
    }
}
