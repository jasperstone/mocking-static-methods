using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentValidation;
using eShop.Ordering.API.Application.Validations;

namespace eShop.Ordering.Tests
{
    public class CreateOrderCommandValidatorTests
    {
        private class DummyCreateOrderCommand
        {
            public string City { get; set; }
            public string Street { get; set; }
            public string State { get; set; }
            public string Country { get; set; }
            public string ZipCode { get; set; }
            public string CardNumber { get; set; }
            public string CardHolderName { get; set; }
            public DateTime CardExpiration { get; set; }
            public string CardSecurityNumber { get; set; }
            public Guid CardTypeId { get; set; }
            public IEnumerable<OrderItemDTO> OrderItems { get; set; }
        }

        private class OrderItemDTO
        {
            // properties as needed
        }

        [Fact]
        public void Constructor_Should_Log_Trace_When_Enabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CreateOrderCommandValidator>>();
            loggerMock.Setup(x => x.IsEnabled(LogLevel.Trace)).Returns(true);
            loggerMock.Setup(x => x.LogTrace(It.IsAny<string>(), It.IsAny<object>()));

            // Act
            var validator = new CreateOrderCommandValidator(loggerMock.Object);

            // Assert
            loggerMock.Verify(x => x.LogTrace("INSTANCE CREATED - {ClassName}", "CreateOrderCommandValidator"), Times.Once);
        }

        [Fact]
        public void Constructor_Should_Not_Log_Trace_When_Disabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CreateOrderCommandValidator>>();
            loggerMock.Setup(x => x.IsEnabled(LogLevel.Trace)).Returns(false);

            // Act
            var validator = new CreateOrderCommandValidator(loggerMock.Object);

            // Assert
            loggerMock.Verify(x => x.LogTrace(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        }
    }
}
