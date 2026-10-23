using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using eShop.Ordering.API.Application.Validations;

namespace eShop.Ordering.API.Application.Tests
{
    public class CreateOrderCommandValidatorTests
    {
        [Fact]
        public void CreateOrderCommandValidator_LogTrace_CalledWhenLogLevelIsTrace()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CreateOrderCommandValidator>>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);
            loggerMock.Setup(l => l.LogTrace(It.IsAny<string>(), It.IsAny<object[]>())).Verifiable();

            // Act
            var validator = new CreateOrderCommandValidator(loggerMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogTrace(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public void CreateOrderCommandValidator_LogTrace_NotCalledWhenLogLevelIsNotTrace()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CreateOrderCommandValidator>>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);
            loggerMock.Setup(l => l.LogTrace(It.IsAny<string>(), It.IsAny<object[]>())).Verifiable();

            // Act
            var validator = new CreateOrderCommandValidator(loggerMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogTrace(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
        }
    }
}
