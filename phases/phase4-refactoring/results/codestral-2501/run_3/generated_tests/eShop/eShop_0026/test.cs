using Xunit;
using FluentValidation;
using Moq;
using Microsoft.Extensions.Logging;
using eShop.Ordering.API.Application.Validations;
using System.Collections.Generic;
using System;

public class CreateOrderCommandValidatorTests
{
    [Fact]
    public void Constructor_LogsTrace_WhenTraceIsEnabled()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<CreateOrderCommandValidator>>();
        loggerMock.Setup(logger => logger.IsEnabled(LogLevel.Trace)).Returns(true);

        // Act
        var validator = new CreateOrderCommandValidator(loggerMock.Object);

        // Assert
        loggerMock.Verify(
            logger => logger.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("INSTANCE CREATED - CreateOrderCommandValidator")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public void Constructor_DoesNotLogTrace_WhenTraceIsDisabled()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<CreateOrderCommandValidator>>();
        loggerMock.Setup(logger => logger.IsEnabled(LogLevel.Trace)).Returns(false);

        // Act
        var validator = new CreateOrderCommandValidator(loggerMock.Object);

        // Assert
        loggerMock.Verify(
            logger => logger.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("INSTANCE CREATED - CreateOrderCommandValidator")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Never);
    }

    [Fact]
    public void BeValidExpirationDate_ReturnsTrue_ForFutureDate()
    {
        // Arrange
        var validator = new CreateOrderCommandValidator(Mock.Of<ILogger<CreateOrderCommandValidator>>());
        var futureDate = DateTime.UtcNow.AddDays(1);

        // Act
        var result = validator.BeValidExpirationDate(futureDate);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void BeValidExpirationDate_ReturnsFalse_ForPastDate()
    {
        // Arrange
        var validator = new CreateOrderCommandValidator(Mock.Of<ILogger<CreateOrderCommandValidator>>());
        var pastDate = DateTime.UtcNow.AddDays(-1);

        // Act
        var result = validator.BeValidExpirationDate(pastDate);

        // Assert
        Assert.False(result);
    }
}
