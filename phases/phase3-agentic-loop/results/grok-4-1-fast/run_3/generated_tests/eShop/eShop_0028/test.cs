using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using FluentValidation;
using eShop.Ordering.API.Application.Validations;
using eShop.Ordering.API.Application;

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
        new ShipOrderCommandValidator(loggerMock.Object);
        
        // Assert
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("INSTANCE CREATED - ShipOrderCommandValidator")),
                null!,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void Constructor_WhenTraceLoggingDisabled_DoesNotCallLogTrace()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ShipOrderCommandValidator>>();
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);
        
        // Act
        new ShipOrderCommandValidator(loggerMock.Object);
        
        // Assert
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }
}
