using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using FluentValidation;
using System.Collections.Generic;
using System;
using eShop.Ordering.API.Application.Validations;

namespace eShop.Ordering.API.Application.Validations.Tests
{
    public class CreateOrderCommandValidatorTests
    {
        private readonly Mock<ILogger<CreateOrderCommandValidator>> _loggerMock;
        private readonly CreateOrderCommandValidator _validator;

        public CreateOrderCommandValidatorTests()
        {
            _loggerMock = new Mock<ILogger<CreateOrderCommandValidator>>();
            _validator = new CreateOrderCommandValidator(_loggerMock.Object);
        }

        [Fact]
        public void Constructor_ShouldLogTrace_WhenTraceIsEnabled()
        {
            // Arrange
            _loggerMock.Setup(x => x.IsEnabled(LogLevel.Trace)).Returns(true);

            // Act
            var validator = new CreateOrderCommandValidator(_loggerMock.Object);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("INSTANCE CREATED - CreateOrderCommandValidator")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public void Constructor_ShouldNotLogTrace_WhenTraceIsDisabled()
        {
            // Arrange
            _loggerMock.Setup(x => x.IsEnabled(LogLevel.Trace)).Returns(false);

            // Act
            var validator = new CreateOrderCommandValidator(_loggerMock.Object);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("INSTANCE CREATED - CreateOrderCommandValidator")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Never);
        }

        [Fact]
        public void BeValidExpirationDate_ShouldReturnTrue_WhenDateIsInFuture()
        {
            // Arrange
            var futureDate = DateTime.UtcNow.AddDays(1);

            // Act
            var result = _validator.BeValidExpirationDate(futureDate);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void BeValidExpirationDate_ShouldReturnFalse_WhenDateIsInPast()
        {
            // Arrange
            var pastDate = DateTime.UtcNow.AddDays(-1);

            // Act
            var result = _validator.BeValidExpirationDate(pastDate);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ContainOrderItems_ShouldReturnTrue_WhenOrderItemsArePresent()
        {
            // Arrange
            var orderItems = new List<OrderItemDTO> { new OrderItemDTO() };

            // Act
            var result = _validator.ContainOrderItems(orderItems);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ContainOrderItems_ShouldReturnFalse_WhenOrderItemsAreEmpty()
        {
            // Arrange
            var orderItems = new List<OrderItemDTO>();

            // Act
            var result = _validator.ContainOrderItems(orderItems);

            // Assert
            Assert.False(result);
        }
    }
}
