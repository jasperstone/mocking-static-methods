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
            var loggerMock = new Mock<ILogger<CreateOrderCommandValidator>>();
            loggerMock.Setup(x => x.IsEnabled(LogLevel.Trace)).Returns(true);
            var called = false;
            loggerMock.Setup(x => x.LogTrace(It.IsAny<string>(), It.IsAny<object>()))
                      .Callback<string, object>((msg, arg) => { called = true; });

            // Act
            var validator = new CreateOrderCommandValidator(loggerMock.Object);

            // Assert
            Assert.True(called);
            loggerMock.Verify(x => x.LogTrace("INSTANCE CREATED - {ClassName}", "CreateOrderCommandValidator"), Times.Once);
        }

        [Fact]
        public void Constructor_Should_Not_LogTrace_When_TraceDisabled()
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
