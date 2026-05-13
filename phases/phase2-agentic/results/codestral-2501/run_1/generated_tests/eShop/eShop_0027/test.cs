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
        public void Constructor_ShouldLogTrace_WhenTraceIsEnabled()
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
        public void Constructor_ShouldNotLogTrace_WhenTraceIsDisabled()
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
        public void Constructor_ShouldValidateId_WhenCommandIsProvided()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<IdentifiedCommandValidator>>();
            var command = new IdentifiedCommand<CreateOrderCommand, bool>(new CreateOrderCommand(), Guid.NewGuid());

            // Act
            var validator = new IdentifiedCommandValidator(loggerMock.Object);

            // Assert
            var result = validator.Validate(command);
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, error => error.PropertyName == "Id" && error.ErrorMessage == "'Id' must not be empty.");
        }
    }
}
