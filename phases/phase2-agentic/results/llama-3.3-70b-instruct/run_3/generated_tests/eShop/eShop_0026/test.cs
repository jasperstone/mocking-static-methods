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
        public void CreateOrderCommandValidator_LogTrace_WhenLoggerIsEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CreateOrderCommandValidator>>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);
            var logger = loggerMock.Object;

            // Act
            new CreateOrderCommandValidator(logger);

            // Assert
            loggerMock.Verify(l => l.LogTrace(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public void CreateOrderCommandValidator_DoesNotLogTrace_WhenLoggerIsNotEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CreateOrderCommandValidator>>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);
            var logger = loggerMock.Object;

            // Act
            new CreateOrderCommandValidator(logger);

            // Assert
            loggerMock.Verify(l => l.LogTrace(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
        }

        [Fact]
        public void Validate_ValidCommand_ReturnsNoErrors()
        {
            // Arrange
            var validator = new CreateOrderCommandValidator(new Mock<ILogger<CreateOrderCommandValidator>>().Object);
            var command = new CreateOrderCommand
            {
                City = "City",
                Street = "Street",
                State = "State",
                Country = "Country",
                ZipCode = "ZipCode",
                CardNumber = "CardNumber",
                CardHolderName = "CardHolderName",
                CardExpiration = DateTime.UtcNow.AddDays(1),
                CardSecurityNumber = "123",
                CardTypeId = 1,
                OrderItems = new List<OrderItemDTO> { new OrderItemDTO() }
            };

            // Act
            var result = validator.Validate(command);

            // Assert
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_InvalidCommand_ReturnsErrors()
        {
            // Arrange
            var validator = new CreateOrderCommandValidator(new Mock<ILogger<CreateOrderCommandValidator>>().Object);
            var command = new CreateOrderCommand
            {
                City = "",
                Street = "",
                State = "",
                Country = "",
                ZipCode = "",
                CardNumber = "",
                CardHolderName = "",
                CardExpiration = DateTime.UtcNow.AddDays(-1),
                CardSecurityNumber = "",
                CardTypeId = 0,
                OrderItems = new List<OrderItemDTO>()
            };

            // Act
            var result = validator.Validate(command);

            // Assert
            Assert.False(result.IsValid);
        }
    }
}
