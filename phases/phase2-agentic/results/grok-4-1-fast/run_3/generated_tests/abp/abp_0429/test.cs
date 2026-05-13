using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Logging;
using Xunit;

namespace Volo.Abp.Extensions.Logging;

public class AbpLoggerExtensionsTests
{
    private readonly Mock<ILogger> _loggerMock;

    public AbpLoggerExtensionsTests()
    {
        _loggerMock = new Mock<ILogger>();
    }

    [Fact]
    public void LogWithLevel_Should_Call_LogCritical_When_LogLevel_Is_Critical_WithException()
    {
        // Arrange
        var logLevel = LogLevel.Critical;
        var message = "Critical error occurred";
        var exception = new InvalidOperationException("Test exception");
        var expectedMessage = message;

        // Act
        _loggerMock.Object.LogWithLevel(logLevel, message, exception);

        // Assert
        _loggerMock.Verify(
            x => x.LogCritical(
                It.IsAny<EventId>(),
                It.IsAny<Exception>(),
                It.Is<It.IsAnyType>((v, t) => ((string)v).ToString() == expectedMessage)
            ),
            Times.Once
        );
    }

    [Theory]
    [InlineData(LogLevel.Critical)]
    [InlineData(LogLevel.Error)]
    [InlineData(LogLevel.Warning)]
    [InlineData(LogLevel.Information)]
    [InlineData(LogLevel.Trace)]
    [InlineData(LogLevel.Debug)]
    public void LogWithLevel_Should_Call_Correct_LogMethod_For_Each_LogLevel_WithException(LogLevel logLevel)
    {
        // Arrange
        var message = "Test message";
        var exception = new Exception("Test exception");

        // Act
        _loggerMock.Object.LogWithLevel(logLevel, message, exception);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == logLevel),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );
    }

    [Fact]
    public void LogWithLevel_Should_Call_LogDebug_For_Default_LogLevel_WithException()
    {
        // Arrange
        var logLevel = LogLevel.None; // Maps to default case
        var message = "Debug message";
        var exception = new Exception("Test exception");

        // Act
        _loggerMock.Object.LogWithLevel(logLevel, message, exception);

        // Assert
        _loggerMock.Verify(
            x => x.LogDebug(
                It.IsAny<EventId>(),
                It.IsAny<Exception>(),
                It.Is<It.IsAnyType>((v, t) => ((string)v).ToString() == message)
            ),
            Times.Once
        );
    }

    [Fact]
    public void LogWithLevel_Should_Call_LogCritical_When_LogLevel_Is_Critical_WithoutException()
    {
        // Arrange
        var logLevel = LogLevel.Critical;
        var message = "Critical message";

        // Act
        _loggerMock.Object.LogWithLevel(logLevel, message);

        // Assert
        _loggerMock.Verify(
            x => x.LogCritical(
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => ((string)v).ToString() == message)
            ),
            Times.Once
        );
    }

    [Fact]
    public void LogException_Should_Call_LogWithLevel_With_ExceptionLogLevel()
    {
        // Arrange
        var exception = new Exception("Test exception") { Data = { { "TestKey", "TestValue" } } };
        var expectedLevel = LogLevel.Error;

        // Mock Exception.GetLogLevel() via subclass
        var mockException = new MockException("Test exception", expectedLevel) { Data = { { "TestKey", "TestValue" } } };

        // Act
        _loggerMock.Object.LogException(mockException);

        // Assert
        _loggerMock.Verify(
            x => x.LogWithLevel(
                expectedLevel,
                mockException.Message,
                mockException
            ),
            Times.Once
        );
    }

    private class MockException : Exception
    {
        private readonly LogLevel _logLevel;

        public MockException(string message, LogLevel logLevel) : base(message)
        {
            _logLevel = logLevel;
        }

        public override LogLevel GetLogLevel() => _logLevel;
    }
}
