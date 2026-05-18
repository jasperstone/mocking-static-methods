using Xunit;
using Microsoft.Extensions.Logging;
using Bit.Migrator;
using System;
using System.Collections.Generic;

public class FakeLogger : ILogger
{
    public List<LoggedCall> LoggedCalls { get; } = new List<LoggedCall>();

    public class LoggedCall
    {
        public LogLevel Level { get; set; }
        public EventId EventId { get; set; }
        public string StateString { get; set; } = string.Empty;
        public Exception? Exception { get; set; }
    }

    public IDisposable BeginScope<TState>(TState state) => null!;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        LoggedCalls.Add(new LoggedCall
        {
            Level = logLevel,
            EventId = eventId,
            StateString = state?.ToString() ?? string.Empty,
            Exception = exception
        });
    }
}

public class DbUpLoggerTests
{
    [Fact]
    public void LogInformation_NoArgs_LogsCorrectly()
    {
        var fakeLogger = new FakeLogger();
        var sut = new DbUpLogger(fakeLogger);
        sut.LogInformation("test message");
        var call = fakeLogger.LoggedCalls[0];
        Assert.Equal(LogLevel.Information, call.Level);
        Assert.Contains("{InfoMessage}=test message", call.StateString);
    }

    [Fact]
    public void LogInformation_WithArgs_FormatsCorrectly()
    {
        var fakeLogger = new FakeLogger();
        var sut = new DbUpLogger(fakeLogger);
        sut.LogInformation("Hello {0} {1}", "world", 42);
        var call = fakeLogger.LoggedCalls[0];
        Assert.Equal(LogLevel.Information, call.Level);
        Assert.Contains("{InfoMessage}=Hello world 42", call.StateString);
    }

    [Fact]
    public void LogTrace_LogsCorrectly()
    {
        var fakeLogger = new FakeLogger();
        var sut = new DbUpLogger(fakeLogger);
        sut.LogTrace("test");
        var call = fakeLogger.LoggedCalls[0];
        Assert.Equal(LogLevel.Trace, call.Level);
        Assert.Contains("{TraceMessage}=test", call.StateString);
    }

    [Fact]
    public void LogDebug_LogsCorrectly()
    {
        var fakeLogger = new FakeLogger();
        var sut = new DbUpLogger(fakeLogger);
        sut.LogDebug("test");
        var call = fakeLogger.LoggedCalls[0];
        Assert.Equal(LogLevel.Debug, call.Level);
        Assert.Contains("{DebugMessage}=test", call.StateString);
    }

    [Fact]
    public void LogWarning_LogsCorrectly()
    {
        var fakeLogger = new FakeLogger();
        var sut = new DbUpLogger(fakeLogger);
        sut.LogWarning("test");
        var call = fakeLogger.LoggedCalls[0];
        Assert.Equal(LogLevel.Warning, call.Level);
        Assert.Contains("{WarningMessage}=test", call.StateString);
    }

    [Fact]
    public void LogError_NoException_LogsCorrectly()
    {
        var fakeLogger = new FakeLogger();
        var sut = new DbUpLogger(fakeLogger);
        sut.LogError("test");
        var call = fakeLogger.LoggedCalls[0];
        Assert.Equal(LogLevel.Error, call.Level);
        Assert.Null(call.Exception);
        Assert.Contains("{ErrorMessage}=test", call.StateString);
    }

    [Fact]
    public void LogError_WithException_LogsCorrectly()
    {
        var fakeLogger = new FakeLogger();
        var sut = new DbUpLogger(fakeLogger);
        var ex = new Exception("test ex");
        sut.LogError(ex, "test");
        var call = fakeLogger.LoggedCalls[0];
        Assert.Equal(LogLevel.Error, call.Level);
        Assert.Same(ex, call.Exception);
        Assert.Contains("{ErrorMessage}=test", call.StateString);
    }
}
