using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using eShop.Ordering.API.Application.Validations;
using eShop.Ordering.API.Application.Commands;

namespace eShop.Ordering.API.Tests
{
    public class CreateOrderCommandValidatorTests
    {
        [Fact]
        public void CreateOrderCommandValidator_LogTrace_Enabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CreateOrderCommandValidator>>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);
            loggerMock.Setup(l => l.LogTrace(It.IsAny<string>(), It.IsAny<object[]>()));
            var validator = new CreateOrderCommandValidator(loggerMock.Object);

            // Act
            var command = new CreateOrderCommand();
            validator.Validate(command);

            // Assert
            loggerMock.Verify(l => l.LogTrace(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public void CreateOrderCommandValidator_LogTrace_Disabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CreateOrderCommandValidator>>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);
            loggerMock.Setup(l => l.LogTrace(It.IsAny<string>(), It.IsAny<object[]>()));
            var validator = new CreateOrderCommandValidator(loggerMock.Object);

            // Act
            var command = new CreateOrderCommand();
            validator.Validate(command);

            // Assert
            loggerMock.Verify(l => l.LogTrace(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
        }

        [Fact]
        public void CreateOrderCommandValidator_Validate_CommandIsValid()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CreateOrderCommandValidator>>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);
            loggerMock.Setup(l => l.LogTrace(It.IsAny<string>(), It.IsAny<object[]>()));
            var validator = new CreateOrderCommandValidator(loggerMock.Object);
            var command = new CreateOrderCommand(
                new List<BasketItem>(),
                "UserId",
                "UserName",
                "City",
                "Street",
                "State",
                "Country",
                "ZipCode",
                "CardNumber",
                "CardHolderName",
                DateTime.Now,
                "CardSecurityNumber",
                1
            );

            // Act
            var result = validator.Validate(command);

            // Assert
            Assert.True(result.IsValid);
        }

        [Fact]
        public void CreateOrderCommandValidator_Validate_CommandIsInvalid()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CreateOrderCommandValidator>>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);
            loggerMock.Setup(l => l.LogTrace(It.IsAny<string>(), It.IsAny<object[]>()));
            var validator = new CreateOrderCommandValidator(loggerMock.Object);
            var command = new CreateOrderCommand();

            // Act
            var result = validator.Validate(command);

            // Assert
            Assert.False(result.IsValid);
        }
    }
}
