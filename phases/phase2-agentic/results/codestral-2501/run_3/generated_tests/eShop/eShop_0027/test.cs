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

            // Act
            var validator = new IdentifiedCommandValidator(loggerMock.Object);

            // Assert
            loggerMock.Verify(logger => logger.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("INSTANCE CREATED - IdentifiedCommandValidator")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }

        [Fact]
        public void LogTrace_ShouldNotBeCalled_WhenLogLevelIsNotTrace()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<IdentifiedCommandValidator>>();
            loggerMock.Setup(logger => logger.IsEnabled(LogLevel.Trace)).Returns(false);

            // Act
            var validator = new IdentifiedCommandValidator(loggerMock.Object);

            // Assert
            loggerMock.Verify(logger => logger.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("INSTANCE CREATED - IdentifiedCommandValidator")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Never);
        }

        [Fact]
        public void RuleForId_ShouldNotBeEmpty()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<IdentifiedCommandValidator>>();
            var validator = new IdentifiedCommandValidator(loggerMock.Object);

            // Act
            var command = new IdentifiedCommand<CreateOrderCommand, bool>(new CreateOrderCommand(), Guid.NewGuid());
            var result = validator.Validate(command);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, error => error.PropertyName == "Id" && error.ErrorMessage == "'Id' must not be empty.");
        }
    }
}
