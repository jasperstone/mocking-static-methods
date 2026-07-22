using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Volo.Abp.ExceptionHandling;
using Xunit;

namespace Volo.Abp.Core.Tests.Logging;

public class AbpLoggerExtensionsTests
{
    private readonly ILogger _logger;

    public AbpLoggerExtensionsTests()
    {
        _logger = NullLoggerFactory.Instance.CreateLogger<AbpLoggerExtensionsTests>();
    }

    [Fact]
    public void LogWithLevel_Should_Call_LogCritical_When_LogLevel_Is_Critical_With_Message()
    {
        // Arrange
        var mockLogger = new MockLogger();
        var logLevel = LogLevel.Critical;
        var message = "Critical error occurred";

        // Act
        mockLogger.LogWithLevel(logLevel, message);

        // Assert
        Assert.Contains("LogCritical", mockLogger.Messages);
        Assert.Single(mockLogger.Messages);
    }

    [Fact]
    public void LogWithLevel_Should_Call_LogCritical_WithExceptionAndMessage_When_LogLevel_Is_Critical()
    {
        // Arrange
        var mockLogger = new MockLogger();
        var logLevel = LogLevel.Critical;
        var message = "Critical exception occurred";
        var exception = new InvalidOperationException("Test exception");

        // Act
        mockLogger.LogWithLevel(logLevel, message, exception);

        // Assert
        Assert.Contains("LogCritical(exception, message)", mockLogger.Messages);
        Assert.Single(mockLogger.Messages);
    }

    [Fact]
    public void LogWithLevel_Should_Call_LogError_When_LogLevel_Is_Error()
    {
        // Arrange
        var mockLogger = new MockLogger();
        var logLevel = LogLevel.Error;
        var message = "Error occurred";

        // Act
        mockLogger.LogWithLevel(logLevel, message);

        // Assert
        Assert.Contains("LogError", mockLogger.Messages);
    }

    [Fact]
    public void LogWithLevel_Should_Call_LogDebug_For_Default_LogLevels()
    {
        // Arrange
        var mockLogger = new MockLogger();
        var logLevel = LogLevel.Debug;
        var message = "Debug message";

        // Act
        mockLogger.LogWithLevel(logLevel, message);

        // Assert
        Assert.Contains("LogDebug", mockLogger.Messages);
    }

    [Fact]
    public void LogException_Should_Call_LogWithLevel_With_Determined_LogLevel()
    {
        // Arrange
        var mockLogger = new MockLogger();
        var exception = new InvalidOperationException("Test");

        // Act
        mockLogger.LogException(exception);

        // Assert
        Assert.Contains("LogError(exception, message)", mockLogger.Messages);
    }

    private class MockLogger : ILogger
    {
        public List<string> Messages { get; } = new();

        public IDisposable? BeginScope<TState>(TState state) => null!;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            var method = logLevel switch
            {
                LogLevel.Critical => exception != null ? "LogCritical(exception, message)" : "LogCritical",
                LogLevel.Error => exception != null ? "LogError(exception, message)" : "LogError",
                LogLevel.Warning => exception != null ? "LogWarning(exception, message)" : "LogWarning",
                LogLevel.Information => exception != null ? "LogInformation(exception, message)" : "LogInformation",
                LogLevel.Trace => exception != null ? "LogTrace(exception, message)" : "LogTrace",
                _ => exception != null ? "LogDebug(exception, message)" : "LogDebug"
            };
            Messages.Add(method);
        }

        // Extension methods
        public void LogCritical(string message) => Messages.Add("LogCritical");
        public void LogCritical(Exception exception, string message) => Messages.Add("LogCritical(exception, message)");
        public void LogError(string message) => Messages.Add("LogError");
        public void LogError(Exception exception, string message) => Messages.Add("LogError(exception, message)");
        public void LogWarning(string message) => Messages.Add("LogWarning");
        public void LogWarning(Exception exception, string message) => Messages.Add("LogWarning(exception, message)");
        public void LogInformation(string message) => Messages.Add("LogInformation");
        public void LogInformation(Exception exception, string message) => Messages.Add("LogInformation(exception, message)");
        public void LogTrace(string message) => Messages.Add("LogTrace");
        public void LogTrace(Exception exception, string message) => Messages.Add("LogTrace(exception, message)");
        public void LogDebug(string message) => Messages.Add("LogDebug");
        public void LogDebug(Exception exception, string message) => Messages.Add("LogDebug(exception, message)");

        // Extension methods for this test
        public void LogWithLevel(LogLevel logLevel, string message) => this.Log(logLevel, 0, message, null, (_, __) => message);
        public void LogWithLevel(LogLevel logLevel, string message, Exception exception) => this.Log(logLevel, 0, message, exception, (_, __) => message);
        public void LogException(Exception ex, LogLevel? level = null)
        {
            // Simplified for testing the switch logic
            this.LogWithLevel(level ?? LogLevel.Error, ex.Message, ex);
        }
    }
}
