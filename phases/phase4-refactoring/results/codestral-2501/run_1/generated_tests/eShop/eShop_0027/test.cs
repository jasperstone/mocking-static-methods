using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using eShop.Ordering.API.Application.Validations;
using FluentValidation;

namespace eShop.Ordering.API.Application.Validations.Tests
{
    public class IdentifiedCommandValidatorTests
    {
        [Fact]
        public void Constructor_ShouldLogTrace_WhenLoggerIsEnabledForTrace()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<IdentifiedCommandValidator>>();
            mockLogger.Setup(logger => logger.IsEnabled(LogLevel.Trace)).Returns(true);

            // Act
            var validator = new IdentifiedCommandValidator(mockLogger.Object);

            // Assert
            mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("INSTANCE CREATED - IdentifiedCommandValidator")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public void Constructor_ShouldNotLogTrace_WhenLoggerIsNotEnabledForTrace()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<IdentifiedCommandValidator>>();
            mockLogger.Setup(logger => logger.IsEnabled(LogLevel.Trace)).Returns(false);

            // Act
            var validator = new IdentifiedCommandValidator(mockLogger.Object);

            // Assert
            mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("INSTANCE CREATED - IdentifiedCommandValidator")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Never);
        }
    }
}
