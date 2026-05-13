using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using eShop.Ordering.API.Application.Validations;
using System;

namespace eShop.Ordering.API.Tests
{
    public class CreateOrderCommandValidatorTests
    {
        [Fact]
        public void Constructor_ShouldLogTrace_WhenLogLevelIsTrace()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CreateOrderCommandValidator>>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);

            // Act
            var validator = new CreateOrderCommandValidator(loggerMock.Object);

            // Assert
            loggerMock.Verify(
                l => l.LogTrace("INSTANCE CREATED - {ClassName}", It.IsAny<string>()),
                Times.Once);
        }

        [Fact]
        public void Constructor_ShouldNotLogTrace_WhenLogLevelIsNotTrace()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CreateOrderCommandValidator>>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);

            // Act
            var validator = new CreateOrderCommandValidator(loggerMock.Object);

            // Assert
            loggerMock.Verify(
                l => l.LogTrace("INSTANCE CREATED - {ClassName}", It.IsAny<string>()),
                Times.Never);
        }
    }
}
