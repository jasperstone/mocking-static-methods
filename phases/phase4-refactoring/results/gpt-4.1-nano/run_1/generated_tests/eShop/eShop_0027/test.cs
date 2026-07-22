using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using FluentValidation;
using eShop.Ordering.API.Application.Validations;
using System;

namespace eShop.Ordering.Tests
{
    public class IdentifiedCommandValidatorTests
    {
        [Fact]
        public void Constructor_ShouldLogTrace_WhenTraceEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<IdentifiedCommandValidator>>();
            loggerMock.Setup(x => x.IsEnabled(LogLevel.Trace)).Returns(true);
            loggerMock.Setup(x => x.LogTrace(It.IsAny<string>(), It.IsAny<object>()))
                      .Verifiable();

            // Act
            var validator = new IdentifiedCommandValidator(loggerMock.Object);

            // Assert
            loggerMock.Verify(x => x.LogTrace("INSTANCE CREATED - {ClassName}", nameof(IdentifiedCommandValidator)), Times.Once);
        }

        [Fact]
        public void Constructor_ShouldNotLogTrace_WhenTraceDisabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<IdentifiedCommandValidator>>();
            loggerMock.Setup(x => x.IsEnabled(LogLevel.Trace)).Returns(false);

            // Act
            var validator = new IdentifiedCommandValidator(loggerMock.Object);

            // Assert
            loggerMock.Verify(x => x.LogTrace(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        }
    }
}
