using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using eShop.Ordering.API.Application.Validations;
using eShop.Ordering.API.Application.Commands;
using FluentValidation;

namespace eShop.Ordering.API.Application.Tests
{
    public class IdentifiedCommandValidatorTests
    {
        [Fact]
        public void IdentifiedCommandValidator_LogTrace_WhenLoggerIsEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<IdentifiedCommandValidator>>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);
            var logger = loggerMock.Object;

            // Act
            var validator = new IdentifiedCommandValidator(logger);

            // Assert
            loggerMock.Verify(l => l.LogTrace("INSTANCE CREATED - {ClassName}", "IdentifiedCommandValidator"), Times.Once);
        }

        [Fact]
        public void IdentifiedCommandValidator_DoNotLogTrace_WhenLoggerIsNotEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<IdentifiedCommandValidator>>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);
            var logger = loggerMock.Object;

            // Act
            var validator = new IdentifiedCommandValidator(logger);

            // Assert
            loggerMock.Verify(l => l.LogTrace(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
        }

        [Fact]
        public void IdentifiedCommandValidator_Validate_CommandIdNotEmpty()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<IdentifiedCommandValidator>>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);
            var logger = loggerMock.Object;
            var validator = new IdentifiedCommandValidator(logger);
            var command = new IdentifiedCommand<CreateOrderCommand, bool>(new CreateOrderCommand(), Guid.NewGuid());

            // Act
            var result = validator.Validate(command);

            // Assert
            Assert.True(result.IsValid);
        }

        [Fact]
        public void IdentifiedCommandValidator_Validate_CommandIdEmpty()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<IdentifiedCommandValidator>>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);
            var logger = loggerMock.Object;
            var validator = new IdentifiedCommandValidator(logger);
            var command = new IdentifiedCommand<CreateOrderCommand, bool>(new CreateOrderCommand(), Guid.Empty);

            // Act
            var result = validator.Validate(command);

            // Assert
            Assert.False(result.IsValid);
        }
    }
}
