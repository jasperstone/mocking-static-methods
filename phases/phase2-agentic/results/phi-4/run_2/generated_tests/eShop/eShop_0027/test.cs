using Moq;
using Microsoft.Extensions.Logging;
using Xunit;
using eShop.Ordering.API.Application.Validations;

namespace eShop.Ordering.API.Tests.Application.Validations
{
    public class IdentifiedCommandValidatorTests
    {
        [Fact]
        public void Constructor_LogsTrace_WhenLoggerIsEnabledForTrace()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<IdentifiedCommandValidator>>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);

            // Act
            var validator = new IdentifiedCommandValidator(loggerMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogTrace("INSTANCE CREATED - {ClassName}", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void Constructor_DoesNotLogTrace_WhenLoggerIsNotEnabledForTrace()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<IdentifiedCommandValidator>>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);

            // Act
            var validator = new IdentifiedCommandValidator(loggerMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogTrace(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
        }
    }
}
