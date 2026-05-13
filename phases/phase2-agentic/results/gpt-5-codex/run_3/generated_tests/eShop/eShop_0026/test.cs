using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using eShop.Ordering.API.Application.Validations;

namespace eShop.Ordering.API.Tests.Application.Validations
{
    public class CreateOrderCommandValidatorTests
    {
        [Fact]
        public void Constructor_WhenTraceLoggingEnabled_LogsTraceMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CreateOrderCommandValidator>>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);

            // Act
            _ = new CreateOrderCommandValidator(loggerMock.Object);

            // Assert
            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((state, type) => state.ToString() == "INSTANCE CREATED - CreateOrderCommandValidator"),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void Constructor_WhenTraceLoggingDisabled_DoesNotLogTraceMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CreateOrderCommandValidator>>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);

            // Act
            _ = new CreateOrderCommandValidator(loggerMock.Object);

            // Assert
            loggerMock.Verify(
                l => l.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Never);
        }
    }
}
