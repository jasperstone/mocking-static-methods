using System;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using eShop.Ordering.API.Application.Validations;
using FluentValidation;

namespace eShop.Ordering.API.Tests
{
    public class ShipOrderCommandValidatorTests
    {
        [Fact]
        public void Constructor_LogsTrace_WhenLogLevelIsEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ShipOrderCommandValidator>>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);
            var logTraceCalled = false;

            loggerMock
                .Setup(l => l.LogTrace(It.IsAny<string>(), It.IsAny<object[]>()))
                .Callback(() => logTraceCalled = true);

            // Act
            var validator = new ShipOrderCommandValidator(loggerMock.Object);

            // Assert
            Assert.True(logTraceCalled);
        }

        [Fact]
        public void Constructor_DoesNotLogTrace_WhenLogLevelIsNotEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ShipOrderCommandValidator>>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);
            var logTraceCalled = false;

            loggerMock
                .Setup(l => l.LogTrace(It.IsAny<string>(), It.IsAny<object[]>()))
                .Callback(() => logTraceCalled = true);

            // Act
            var validator = new ShipOrderCommandValidator(loggerMock.Object);

            // Assert
            Assert.False(logTraceCalled);
        }
    }
}
