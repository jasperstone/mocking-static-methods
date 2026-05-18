using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using eShop.Ordering.API.Application.Validations;
using eShop.Ordering.API.Application.Commands;
using FluentValidation;

namespace eShop.Ordering.API.Application.Tests.Validations
{
    public class IdentifiedCommandValidatorTests
    {
        [Fact]
        public void LogTrace_ShouldBeCalled_WhenLogLevelIsTrace()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<IdentifiedCommandValidator>>();
            loggerMock.Setup(logger => logger.IsEnabled(LogLevel.Trace)).Returns(true);

            var validator = new IdentifiedCommandValidator(loggerMock.Object);

            // Act
            var command = new IdentifiedCommand<CreateOrderCommand, bool>(new CreateOrderCommand(), Guid.NewGuid());

            // Assert
            loggerMock.Verify(
                logger => logger.LogTrace(
                    It.IsAny<EventId>(),
                    It.IsAny<string>(),
                    It.IsAny<object[]>()),
                Times.Once);
        }

        [Fact]
        public void LogTrace_ShouldNotBeCalled_WhenLogLevelIsNotTrace()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<IdentifiedCommandValidator>>();
            loggerMock.Setup(logger => logger.IsEnabled(LogLevel.Trace)).Returns(false);

            var validator = new IdentifiedCommandValidator(loggerMock.Object);

            // Act
            var command = new IdentifiedCommand<CreateOrderCommand, bool>(new CreateOrderCommand(), Guid.NewGuid());

            // Assert
            loggerMock.Verify(
                logger => logger.LogTrace(
                    It.IsAny<EventId>(),
                    It.IsAny<string>(),
                    It.IsAny<object[]>()),
                Times.Never);
        }
    }
}
