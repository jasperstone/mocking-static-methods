using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using FluentValidation;
using eShop.Ordering.API.Application.Validations;
using System;
using System.Collections.Generic;

namespace eShop.Ordering.API.Tests
{
    public class CreateOrderCommandValidatorTests
    {
        [Fact]
        public void Constructor_ShouldLogTrace_WhenLogLevelIsTrace()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CreateOrderCommandValidator>>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);
            var validator = new CreateOrderCommandValidator(loggerMock.Object);

            // Act
            // No explicit action needed, constructor is called during instantiation

            // Assert
            loggerMock.Verify(l => l.IsEnabled(LogLevel.Trace), Times.Once);
            loggerMock.Verify(l => l.LogTrace("INSTANCE CREATED - {ClassName}", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void Constructor_ShouldNotLogTrace_WhenLogLevelIsNotTrace()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CreateOrderCommandValidator>>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);
            var validator = new CreateOrderCommandValidator(loggerMock.Object);

            // Act
            // No explicit action needed, constructor is called during instantiation

            // Assert
            loggerMock.Verify(l => l.IsEnabled(LogLevel.Trace), Times.Once);
            loggerMock.Verify(l => l.LogTrace(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
        }
    }
}
