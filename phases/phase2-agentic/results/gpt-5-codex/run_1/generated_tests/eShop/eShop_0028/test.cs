using System;
using eShop.Ordering.API.Application.Validations;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace eShop.Ordering.UnitTests.Application.Validations
{
    public class ShipOrderCommandValidatorTests
    {
        [Fact]
        public void Constructor_LogsTraceWhenTraceEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ShipOrderCommandValidator>>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);

            // Act
            _ = new ShipOrderCommandValidator(loggerMock.Object);

            // Assert
            loggerMock.Verify(l => l.IsEnabled(LogLevel.Trace), Times.AtLeastOnce());
            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((state, _) =>
                        state.ToString() == $"INSTANCE CREATED - {nameof(ShipOrderCommandValidator)}"),
                    It.Is<Exception>(ex => ex == null),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void Constructor_DoesNotLogTraceWhenTraceNotEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ShipOrderCommandValidator>>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);

            // Act
            _ = new ShipOrderCommandValidator(loggerMock.Object);

            // Assert
            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }
    }
}
