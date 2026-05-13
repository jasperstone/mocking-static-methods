using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentValidation;
using eShop.Ordering.API.Application.Validations;

namespace eShop.Ordering.Tests
{
    public class IdentifiedCommandValidatorTests
    {
        [Fact]
        public void Constructor_Should_LogTrace_When_LoggerIsEnabledForTrace()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<IdentifiedCommandValidator>>();
            loggerMock.Setup(x => x.IsEnabled(LogLevel.Trace)).Returns(true);
            string loggedMessage = null;
            loggerMock.Setup(x => x.LogTrace(It.IsAny<string>(), It.IsAny<object[]>()))
                      .Callback<string, object[]>((msg, args) =>
                      {
                          loggedMessage = string.Format(msg, args);
                      });

            // Act
            var validator = new IdentifiedCommandValidator(loggerMock.Object);

            // Assert
            Assert.NotNull(validator);
            Assert.Contains("INSTANCE CREATED", loggedMessage);
            Assert.Contains(nameof(IdentifiedCommandValidator), loggedMessage);
        }

        [Fact]
        public void Constructor_Should_NotLogTrace_When_LoggerIsNotEnabledForTrace()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<IdentifiedCommandValidator>>();
            loggerMock.Setup(x => x.IsEnabled(LogLevel.Trace)).Returns(false);

            // Act
            var validator = new IdentifiedCommandValidator(loggerMock.Object);

            // Assert
            // No exception, and no log trace should be called
            loggerMock.Verify(x => x.LogTrace(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
        }
    }
}
