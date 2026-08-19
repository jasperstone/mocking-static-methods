using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using eShop.Ordering.API.Application.Validations;
using eShop.Ordering.API.Application.Commands;

namespace eShop.Ordering.API.Application.Tests
{
    public class ShipOrderCommandValidatorTests
    {
        [Fact]
        public void Validate_OrderNumberIsEmpty_ValidationFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ShipOrderCommandValidator>>();
            var validator = new ShipOrderCommandValidator(loggerMock.Object);
            var command = new ShipOrderCommand(0);

            // Act
            var result = validator.Validate(command);

            // Assert
            Assert.False(result.IsValid);
            Assert.Single(result.Errors);
            Assert.Equal("No orderId found", result.Errors[0].ErrorMessage);
        }

        [Fact]
        public void Validate_OrderNumberIsNotEmpty_ValidationSucceeds()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ShipOrderCommandValidator>>();
            var validator = new ShipOrderCommandValidator(loggerMock.Object);
            var command = new ShipOrderCommand(1);

            // Act
            var result = validator.Validate(command);

            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }
    }
}
