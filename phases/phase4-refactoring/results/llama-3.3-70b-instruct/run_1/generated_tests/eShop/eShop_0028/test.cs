using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using eShop.Ordering.API.Application.Validations;

namespace eShop.Ordering.API.Application.Tests
{
    public class ShipOrderCommandValidatorTests
    {
        [Fact]
        public void Constructor_LogTrace_WhenLogLevelIsTrace()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ShipOrderCommandValidator>>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);
            var validator = new ShipOrderCommandValidator(loggerMock.Object);

            // Act
            // No action needed, LogTrace is called in the constructor

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString().Contains("INSTANCE CREATED - ShipOrderCommandValidator")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
                ), Times.Once);
        }

        [Fact]
        public void Constructor_NoLogTrace_WhenLogLevelIsNotTrace()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ShipOrderCommandValidator>>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);
            var validator = new ShipOrderCommandValidator(loggerMock.Object);

            // Act
            // No action needed, LogTrace is called in the constructor

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString().Contains("INSTANCE CREATED - ShipOrderCommandValidator")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
                ), Times.Never);
        }
    }
}
