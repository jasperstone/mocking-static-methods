using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace eShop.Ordering.API.Application.Validations.Tests
{
    public class CreateOrderCommandValidatorTests
    {
        private static CreateOrderCommand CreateValidCommand()
        {
            return new CreateOrderCommand
            {
                City = "Sample City",
                Street = "123 Main St",
                State = "ST",
                Country = "USA",
                ZipCode = "12345",
                CardNumber = "123456789012",
                CardHolderName = "John Doe",
                CardExpiration = DateTime.UtcNow.AddDays(1),
                CardSecurityNumber = "123",
                CardTypeId = 1,
                OrderItems = new List<OrderItemDTO>
                {
                    new OrderItemDTO()
                }
            };
        }

        [Fact]
        public void Constructor_LogsTraceWhenEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CreateOrderCommandValidator>>();
            loggerMock.Setup(logger => logger.IsEnabled(LogLevel.Trace)).Returns(true);

            // Act
            _ = new CreateOrderCommandValidator(loggerMock.Object);

            // Assert
            loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("INSTANCE CREATED - CreateOrderCommandValidator")),
                    It.IsAny<Exception?>(),
                    (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
                Times.Once);
        }

        [Fact]
        public void Constructor_DoesNotLogTraceWhenDisabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CreateOrderCommandValidator>>();
            loggerMock.Setup(logger => logger.IsEnabled(LogLevel.Trace)).Returns(false);

            // Act
            _ = new CreateOrderCommandValidator(loggerMock.Object);

            // Assert
            loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception?>(),
                    (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
                Times.Never);
        }

        [Fact]
        public void Validate_ValidCommand_ShouldPass()
        {
            // Arrange
            var command = CreateValidCommand();
            var logger = Mock.Of<ILogger<CreateOrderCommandValidator>>(logger => logger.IsEnabled(LogLevel.Trace) == false);
            var validator = new CreateOrderCommandValidator(logger);

            // Act
            var result = validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Validate_InvalidCommand_ShouldFail()
        {
            // Arrange
            var command = new CreateOrderCommand
            {
                OrderItems = Enumerable.Empty<OrderItemDTO>()
            };
            var logger = Mock.Of<ILogger<CreateOrderCommandValidator>>(logger => logger.IsEnabled(LogLevel.Trace) == false);
            var validator = new CreateOrderCommandValidator(logger);

            // Act
            var result = validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.City);
            result.ShouldHaveValidationErrorFor(c => c.Street);
            result.ShouldHaveValidationErrorFor(c => c.State);
            result.ShouldHaveValidationErrorFor(c => c.Country);
            result.ShouldHaveValidationErrorFor(c => c.ZipCode);
            result.ShouldHaveValidationErrorFor(c => c.CardNumber);
            result.ShouldHaveValidationErrorFor(c => c.CardHolderName);
            result.ShouldHaveValidationErrorFor(c => c.CardExpiration);
            result.ShouldHaveValidationErrorFor(c => c.CardSecurityNumber);
            result.ShouldHaveValidationErrorFor(c => c.CardTypeId);
            result.ShouldHaveValidationErrorFor(c => c.OrderItems);
        }
    }
}
