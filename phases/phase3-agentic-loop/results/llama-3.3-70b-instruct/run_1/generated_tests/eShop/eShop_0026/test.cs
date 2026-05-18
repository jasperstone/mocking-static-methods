using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using eShop.Ordering.API.Application.Validations;

namespace eShop.Ordering.API.Tests
{
    public class CreateOrderCommandValidatorTests
    {
        [Fact]
        public void CreateOrderCommandValidator_LogTrace_WhenLoggerIsEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CreateOrderCommandValidator>>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);
            var validator = new CreateOrderCommandValidator(loggerMock.Object);

            // Act
            // No action needed, the log is called in the constructor

            // Assert
            loggerMock.Verify(l => l.LogTrace(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public void CreateOrderCommandValidator_DoNotLogTrace_WhenLoggerIsNotEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CreateOrderCommandValidator>>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);
            var validator = new CreateOrderCommandValidator(loggerMock.Object);

            // Act
            // No action needed, the log is called in the constructor

            // Assert
            loggerMock.Verify(l => l.LogTrace(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
        }
    }
}
