using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using eShop.Ordering.API.Application.Validations;
using FluentValidation.TestHelper;
using System.Collections.Generic;
using System;

namespace eShop.Ordering.API.Application.Validations.Tests
{
    public class CreateOrderCommandValidatorTests
    {
        private readonly CreateOrderCommandValidator _validator;
        private readonly Mock<ILogger<CreateOrderCommandValidator>> _loggerMock;

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
        public void Should_Have_Error_When_City_Is_Empty()
        {
            _validator.ShouldHaveValidationErrorFor(command => command.City, string.Empty);
        }

        [Fact]
        public void Should_Have_Error_When_Street_Is_Empty()
        {
            _validator.ShouldHaveValidationErrorFor(command => command.Street, string.Empty);
        }

        [Fact]
        public void Should_Have_Error_When_State_Is_Empty()
        {
            _validator.ShouldHaveValidationErrorFor(command => command.State, string.Empty);
        }

        [Fact]
        public void Should_Have_Error_When_Country_Is_Empty()
        {
            _validator.ShouldHaveValidationErrorFor(command => command.Country, string.Empty);
        }

        [Fact]
        public void Should_Have_Error_When_ZipCode_Is_Empty()
        {
            _validator.ShouldHaveValidationErrorFor(command => command.ZipCode, string.Empty);
        }

        [Fact]
        public void Should_Have_Error_When_CardNumber_Is_Empty()
        {
            _validator.ShouldHaveValidationErrorFor(command => command.CardNumber, string.Empty);
        }

        [Fact]
        public void Should_Have_Error_When_CardNumber_Is_Too_Short()
        {
            _validator.ShouldHaveValidationErrorFor(command => command.CardNumber, "12345678901");
        }

        [Fact]
        public void Should_Have_Error_When_CardNumber_Is_Too_Long()
        {
            _validator.ShouldHaveValidationErrorFor(command => command.CardNumber, "1234567890123456789");
        }

        [Fact]
        public void Should_Have_Error_When_CardHolderName_Is_Empty()
        {
            _validator.ShouldHaveValidationErrorFor(command => command.CardHolderName, string.Empty);
        }

        [Fact]
        public void Should_Have_Error_When_CardExpiration_Is_Empty()
        {
            _validator.ShouldHaveValidationErrorFor(command => command.CardExpiration, DateTime.MinValue);
        }

        [Fact]
        public void Should_Have_Error_When_CardExpiration_Is_Invalid()
        {
            _validator.ShouldHaveValidationErrorFor(command => command.CardExpiration, DateTime.UtcNow.AddDays(-1));
        }

        [Fact]
        public void Should_Have_Error_When_CardSecurityNumber_Is_Empty()
        {
            _validator.ShouldHaveValidationErrorFor(command => command.CardSecurityNumber, string.Empty);
        }

        [Fact]
        public void Should_Have_Error_When_CardSecurityNumber_Is_Too_Short()
        {
            _validator.ShouldHaveValidationErrorFor(command => command.CardSecurityNumber, "12");
        }

        [Fact]
        public void Should_Have_Error_When_CardSecurityNumber_Is_Too_Long()
        {
            _validator.ShouldHaveValidationErrorFor(command => command.CardSecurityNumber, "1234");
        }

        [Fact]
        public void Should_Have_Error_When_CardTypeId_Is_Empty()
        {
            _validator.ShouldHaveValidationErrorFor(command => command.CardTypeId, 0);
        }

        [Fact]
        public void Should_Have_Error_When_OrderItems_Is_Empty()
        {
            _validator.ShouldHaveValidationErrorFor(command => command.OrderItems, new List<OrderItemDTO>());
        }
    }
}
