using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using eShop.Ordering.API.Application.Validations;
using eShop.Ordering.API.Application.Commands;
using eShop.Ordering.API.Application.Models;
using eShop.Ordering.API.Extensions;

namespace eShop.Ordering.API.Tests
{
    public class CreateOrderCommandValidatorTests
    {
        [Fact]
        public void CreateOrderCommandValidator_LogTrace_WhenLoggerIsEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CreateOrderCommandValidator>>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);
            var logger = loggerMock.Object;

            // Act
            var validator = new CreateOrderCommandValidator(logger);

            // Assert
            loggerMock.Verify(l => l.LogTrace("INSTANCE CREATED - {ClassName}", "CreateOrderCommandValidator"), Times.Once);
        }

        [Fact]
        public void CreateOrderCommandValidator_DoesNotLogTrace_WhenLoggerIsNotEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CreateOrderCommandValidator>>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);
            var logger = loggerMock.Object;

            // Act
            var validator = new CreateOrderCommandValidator(logger);

            // Assert
            loggerMock.Verify(l => l.LogTrace(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
        }

        [Fact]
        public void CreateOrderCommandValidator_ValidatesCreateOrderCommand()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CreateOrderCommandValidator>>();
            var logger = loggerMock.Object;
            var validator = new CreateOrderCommandValidator(logger);
            var command = new CreateOrderCommand();

            // Act
            var result = validator.Validate(command);

            // Assert
            Assert.False(result.IsValid);
        }

        [Fact]
        public void CreateOrderCommandValidator_ValidatesCreateOrderCommand_WithValidData()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CreateOrderCommandValidator>>();
            var logger = loggerMock.Object;
            var validator = new CreateOrderCommandValidator(logger);
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
                DateTime.UtcNow,
                "CardSecurityNumber",
                1
            );

            // Act
            var result = validator.Validate(command);

            // Assert
            Assert.False(result.IsValid);
        }

        [Fact]
        public void CreateOrderCommandValidator_ValidatesCreateOrderCommand_WithValidDataAndOrderItems()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CreateOrderCommandValidator>>();
            var logger = loggerMock.Object;
            var validator = new CreateOrderCommandValidator(logger);
            var basketItems = new List<BasketItem> { new BasketItem() };
            var command = new CreateOrderCommand(
                basketItems,
                "UserId",
                "UserName",
                "City",
                "Street",
                "State",
                "Country",
                "ZipCode",
                "CardNumber",
                "CardHolderName",
                DateTime.UtcNow,
                "CardSecurityNumber",
                1
            );

            // Act
            var result = validator.Validate(command);

            // Assert
            Assert.True(result.IsValid);
        }
    }
}
