using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using eShop.Ordering.API.Application.Validations;
using FluentValidation;

public class IdentifiedCommandValidatorTests
{
    [Fact]
    public void Constructor_LogsTraceMessage_WhenLoggerIsEnabledForTrace()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<IdentifiedCommandValidator>>();
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);
        var logTraceMock = new Mock<ILogger<IdentifiedCommandValidator>>();
        logTraceMock.Setup(l => l.LogTrace(It.IsAny<string>(), It.IsAny<object[]>()))
                    .Callback<string, object[]>((message, args) => 
                    {
                        Assert.Equal("INSTANCE CREATED - IdentifiedCommandValidator", message);
                        Assert.Equal(new[] { typeof(IdentifiedCommandValidator).Name }, args);
                    });

        // Act
        var validator = new IdentifiedCommandValidator(logTraceMock.Object);

        // Assert
        logTraceMock.Verify(l => l.LogTrace("INSTANCE CREATED - {ClassName}", It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    public void Constructor_DoesNotLogTraceMessage_WhenLoggerIsNotEnabledForTrace()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<IdentifiedCommandValidator>>();
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);
        var logTraceMock = new Mock<ILogger<IdentifiedCommandValidator>>();

        // Act
        var validator = new IdentifiedCommandValidator(logTraceMock.Object);

        // Assert
        logTraceMock.Verify(l => l.LogTrace(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
    }
}
