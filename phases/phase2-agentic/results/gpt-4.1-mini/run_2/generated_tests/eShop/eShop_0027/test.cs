using System;
using eShop.Ordering.API.Application.Validations;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace eShop.Ordering.API.Application.Validations.Tests
{
    public class IdentifiedCommandValidatorTests
    {
        private class DummyCommand : IdentifiedCommand<DummyInnerCommand, bool>
        {
            public DummyCommand(Guid id, DummyInnerCommand command) : base(id, command) { }
        }

        private class DummyInnerCommand
        {
        }

        [Fact]
        public void Constructor_LogsTrace_WhenTraceEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<IdentifiedCommandValidator>>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);

            // Act
            var validator = new IdentifiedCommandValidator(loggerMock.Object);

            // Assert
            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("INSTANCE CREATED - IdentifiedCommandValidator")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void Constructor_DoesNotLogTrace_WhenTraceDisabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<IdentifiedCommandValidator>>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);

            // Act
            var validator = new IdentifiedCommandValidator(loggerMock.Object);

            // Assert
            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Never);
        }

        [Fact]
        public void RuleFor_Id_IsNotEmpty()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<IdentifiedCommandValidator>>();
            var validator = new IdentifiedCommandValidator(loggerMock.Object);

            var command = new IdentifiedCommand<DummyInnerCommand, bool>(Guid.Empty, new DummyInnerCommand());

            // Act
            var result = validator.Validate(command);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Id" && e.ErrorMessage.Contains("not empty"));
        }
    }
}
