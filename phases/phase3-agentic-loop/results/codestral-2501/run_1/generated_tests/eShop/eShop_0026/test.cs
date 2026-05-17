using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using eShop.Ordering.API.Application.Validations;
using FluentValidation.TestHelper;

namespace eShop.Ordering.API.Application.Validations.Tests
{
    public class CreateOrderCommandValidatorTests
    {
        [Fact]
        public void LogTrace_ShouldBeCalled_WhenLoggerIsEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CreateOrderCommandValidator>>();
            loggerMock.Setup(x => x.IsEnabled(LogLevel.Trace)).Returns(true);

            // Act
            var validator = new CreateOrderCommandValidator(loggerMock.Object);

            // Assert
            loggerMock.Verify(
                x => x.LogTrace("INSTANCE CREATED - {ClassName}", It.IsAny<object[]>()),
                Times.Once);
        }

        [Fact]
        public void LogTrace_ShouldNotBeCalled_WhenLoggerIsNotEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CreateOrderCommandValidator>>();
            loggerMock.Setup(x => x.IsEnabled(LogLevel.Trace)).Returns(false);

            // Act
            var validator = new CreateOrderCommandValidator(loggerMock.Object);

            // Assert
            loggerMock.Verify(
                x => x.LogTrace("INSTANCE CREATED - {ClassName}", It.IsAny<object[]>()),
                Times.Never);
        }

        [Fact]
        public void Should_Have_Error_When_City_Is_Empty()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CreateOrderCommandValidator>>();
            var validator = new CreateOrderCommandValidator(loggerMock.Object);
            var command = new CreateOrderCommand { City = "" };

            // Act
            var result = validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.City);
        }
    }
}
