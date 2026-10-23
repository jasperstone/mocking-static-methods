using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using eShop.Ordering.API.Application.Validations;

namespace eShop.Ordering.API.Application.Tests
{
    public class IdentifiedCommandValidatorTests
    {
        [Fact]
        public void IdentifiedCommandValidator_LogTrace_WhenLoggerIsEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<IdentifiedCommandValidator>>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);
            var validator = new IdentifiedCommandValidator(loggerMock.Object);

            // Act
            // No action needed, LogTrace is called in the constructor

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.IsAny<object>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<object, Exception, string>>()
            ), Times.Once);
        }

        [Fact]
        public void IdentifiedCommandValidator_LogTrace_WhenLoggerIsNotEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<IdentifiedCommandValidator>>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);
            var validator = new IdentifiedCommandValidator(loggerMock.Object);

            // Act
            // No action needed, LogTrace is called in the constructor

            // Assert
            loggerMock.Verify(l => l.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<object>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<object, Exception, string>>()
            ), Times.Never);
        }
    }
}
