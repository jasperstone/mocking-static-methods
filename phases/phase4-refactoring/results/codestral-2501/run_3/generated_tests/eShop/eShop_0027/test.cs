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
        public void Constructor_ShouldLogTrace_WhenTraceIsEnabled()
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
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void Constructor_ShouldNotLogTrace_WhenTraceIsNotEnabled()
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
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Never);
        }

        [Fact]
        public void SimpleTest()
        {
            Assert.True(true);
        }
    }
}
