using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using FluentValidation;
using eShop.Ordering.API.Application.Validations;

namespace eShop.Tests
{
    public class IdentifiedCommandValidatorTests
    {
        [Fact]
        public void Constructor_ShouldLogTrace_WhenTraceEnabled()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<IdentifiedCommandValidator>>();
            mockLogger.Setup(x => x.IsEnabled(LogLevel.Trace)).Returns(true);
            mockLogger.Setup(x => x.LogTrace(It.IsAny<string>(), It.IsAny<object>()));

            // Act
            var validator = new IdentifiedCommandValidator(mockLogger.Object);

            // Assert
            mockLogger.Verify(x => x.LogTrace("INSTANCE CREATED - {ClassName}", "IdentifiedCommandValidator"), Times.Once);
        }

        [Fact]
        public void Constructor_ShouldNotLogTrace_WhenTraceDisabled()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<IdentifiedCommandValidator>>();
            mockLogger.Setup(x => x.IsEnabled(LogLevel.Trace)).Returns(false);

            // Act
            var validator = new IdentifiedCommandValidator(mockLogger.Object);

            // Assert
            mockLogger.Verify(x => x.LogTrace(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        }
    }
}
