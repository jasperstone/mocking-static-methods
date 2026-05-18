using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using eShop.Ordering.API.Application.Validations;
using eShop.Ordering.API.Application.Commands;
using FluentValidation;

namespace eShop.Ordering.API.Tests
{
    public class IdentifiedCommandValidatorTests
    {
        [Fact]
        public void IdentifiedCommandValidator_LogsTrace_WhenLoggerIsEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<IdentifiedCommandValidator>>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);
            loggerMock.Setup(l => l.LogTrace(It.IsAny<string>(), It.IsAny<object[]>()));
            var validator = new IdentifiedCommandValidator(loggerMock.Object);

            // Act and Assert
            loggerMock.Verify(l => l.LogTrace("INSTANCE CREATED - {ClassName}", "IdentifiedCommandValidator"), Times.Once);
        }

        [Fact]
        public void IdentifiedCommandValidator_DoesNotLogTrace_WhenLoggerIsNotEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<IdentifiedCommandValidator>>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);
            loggerMock.Setup(l => l.LogTrace(It.IsAny<string>(), It.IsAny<object[]>()));
            var validator = new IdentifiedCommandValidator(loggerMock.Object);

            // Act and Assert
            loggerMock.Verify(l => l.LogTrace(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
        }
    }
}
