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
        public void Constructor_Should_LogTrace_When_TraceEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<IdentifiedCommandValidator>>();
            loggerMock.Setup(x => x.IsEnabled(LogLevel.Trace)).Returns(true);
            var validator = new IdentifiedCommandValidator(loggerMock.Object);

            // Act
            // No explicit action needed, constructor runs

            // Assert
            loggerMock.Verify(x => x.LogTrace("INSTANCE CREATED - {ClassName}", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void Constructor_Should_NotLogTrace_When_TraceDisabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<IdentifiedCommandValidator>>();
            loggerMock.Setup(x => x.IsEnabled(LogLevel.Trace)).Returns(false);
            var validator = new IdentifiedCommandValidator(loggerMock.Object);

            // Act
            // No explicit action needed, constructor runs

            // Assert
            loggerMock.Verify(x => x.LogTrace(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
        }
    }
}
