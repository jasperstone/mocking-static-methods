using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using eShop.Ordering.API.Application.Validations;
using eShop.Ordering.API.Application.Commands;
using eShop.Ordering.API.Application.Models;

namespace eShop.Ordering.API.Application.Tests
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
            loggerMock.Verify(l => l.LogTrace(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public void CreateOrderCommandValidator_DoNotLogTrace_WhenLoggerIsNotEnabled()
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
        public void CreateOrderCommandValidator_ValidateCommand_WithValidData()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CreateOrderCommandValidator>>();
            var logger = loggerMock.Object;
            var basketItem = new BasketItem
            {
                ProductId = 1,
                ProductName = "Product",
                UnitPrice = 10.99m,
                OldUnitPrice = 12.99m,
                Quantity = 2,
                PictureUrl = "https://example.com/image.jpg"
            };
            var command = new CreateOrderCommand(
                new List<BasketItem> { basketItem },
                "userId",
                "userName",
                "city",
                "street",
                "state",
                "country",
                "zipcode",
                "cardNumber",
                "cardHolderName",
                DateTime.Now,
                "cardSecurityNumber",
                1);

            // Act
            var validator = new CreateOrderCommandValidator(logger);
            var result = validator.Validate(command);

            // Assert
            Assert.True(result.IsValid);
        }

        [Fact]
        public void CreateOrderCommandValidator_ValidateCommand_WithInvalidData()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CreateOrderCommandValidator>>();
            var logger = loggerMock.Object;
            var basketItem = new BasketItem
            {
                ProductId = 1,
                ProductName = "Product",
                UnitPrice = 10.99m,
                OldUnitPrice = 12.99m,
                Quantity = 2,
                PictureUrl = "https://example.com/image.jpg"
            };
            var command = new CreateOrderCommand(
                new List<BasketItem> { basketItem },
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                DateTime.Now,
                string.Empty,
                0);

            // Act
            var validator = new CreateOrderCommandValidator(logger);
            var result = validator.Validate(command);

            // Assert
            Assert.False(result.IsValid);
        }
    }
}
