using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using FluentValidation;
using System;
using System.Collections.Generic;
using eShop.Ordering.API.Application.Validations;

namespace eShop.Tests
{
    public class CreateOrderCommandValidatorTests
    {
        [Fact]
        public void Constructor_Should_LogTrace_When_TraceEnabled()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<CreateOrderCommandValidator>>();
            mockLogger.Setup(x => x.IsEnabled(LogLevel.Trace)).Returns(true);
            string loggedMessage = null;
            mockLogger.Setup(x => x.Log(LogLevel.Trace, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), null, It.IsAny<Func<It.IsAnyType, Exception, string>>()))
                .Callback<LogLevel, EventId, object, Exception, Func<It.IsAnyType, Exception, string>>((level, eventId, state, exception, formatter) =>
                {
                    loggedMessage = formatter(state, exception);
                });
            // Act
            var validator = new CreateOrderCommandValidator(mockLogger.Object);
            // Assert
            Assert.NotNull(validator);
            Assert.Contains("INSTANCE CREATED", loggedMessage);
        }

        [Fact]
        public void Constructor_Should_Not_Log_When_TraceDisabled()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<CreateOrderCommandValidator>>();
            mockLogger.Setup(x => x.IsEnabled(LogLevel.Trace)).Returns(false);
            // Act
            var validator = new CreateOrderCommandValidator(mockLogger.Object);
            // Assert
            mockLogger.Verify(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), null, It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Never);
        }
    }
}
