using System.Collections.Generic;
using System.Linq;
using eShop.Ordering.API.Application.Validations;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace eShop.Ordering.UnitTests.Application.Validations
{
    public class IdentifiedCommandValidatorTests
    {
        [Fact]
        public void Constructor_LogsTraceMessageWhenTraceEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<IdentifiedCommandValidator>>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);

            // Act
            _ = new IdentifiedCommandValidator(loggerMock.Object);

            // Assert
            loggerMock.Verify(l => l.IsEnabled(LogLevel.Trace), Times.Once);

            var logInvocation = loggerMock.Invocations.SingleOrDefault(invocation => invocation.Method.Name == "Log");

            Assert.NotNull(logInvocation);
            Assert.Equal(LogLevel.Trace, (LogLevel)logInvocation!.Arguments[0]);

            var state = logInvocation.Arguments[2];
            Assert.NotNull(state);
            Assert.Equal("INSTANCE CREATED - IdentifiedCommandValidator", state!.ToString());

            var structuredValues = Assert.IsAssignableFrom<IReadOnlyList<KeyValuePair<string, object?>>>(state);
            Assert.Contains(structuredValues, kvp => kvp.Key == "ClassName" && kvp.Value is string className && className == nameof(IdentifiedCommandValidator));
        }

        [Fact]
        public void Constructor_DoesNotLogTraceWhenTraceDisabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<IdentifiedCommandValidator>>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);

            // Act
            _ = new IdentifiedCommandValidator(loggerMock.Object);

            // Assert
            loggerMock.Verify(l => l.IsEnabled(LogLevel.Trace), Times.Once);
            Assert.DoesNotContain(loggerMock.Invocations, invocation => invocation.Method.Name == "Log");
        }
    }
}
