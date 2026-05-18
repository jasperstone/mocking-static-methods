using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Microsoft.CodeAnalysis.MSBuild.UnitTests;

public class LoggerExtensionsTests
{
    [Fact]
    public void LogInformation_CallsWithExpectedMessageTemplate()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var logger = mockLogger.Object;
        var processPath = "/current/process/path";
        var dotnetPath = "/different/sdk/dotnet";
        var messageTemplate = ".NET BuildHost started from {ProcessPath} reloading to start from {DotnetPath} to match necessary SDK location.";

        // Act - Exact extension method call from line 157
        logger.LogInformation(messageTemplate, processPath, dotnetPath);

        // Assert
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogInformation_NullLogger_Safe()
    {
        // Arrange
        ILogger? logger = null;
        var processPath = "/current/process/path";
        var dotnetPath = "/different/sdk/dotnet";

        // Act & Assert - Matches the _logger?.LogInformation pattern
        logger?.LogInformation(
            ".NET BuildHost started from {ProcessPath} reloading to start from {DotnetPath} to match necessary SDK location.",
            processPath, dotnetPath);
    }

    [Fact]
    public void LogInformation_ValidatesMessageTemplateFormat()
    {
        // Arrange
        var loggerProvider = new ListLoggerProvider();
        using var loggerFactory = LoggerFactory.Create(b => b.AddProvider(loggerProvider));
        var logger = loggerFactory.CreateLogger("TestCategory");

        var processPath = "old/path";
        var dotnetPath = "new/path";
        var messageTemplate = ".NET BuildHost started from {ProcessPath} reloading to start from {DotnetPath} to match necessary SDK location.";

        // Act
        logger.LogInformation(messageTemplate, processPath, dotnetPath);

        // Assert
        var logEntry = Assert.Single(loggerProvider.Logs);
        Assert.Equal(LogLevel.Information, logEntry.Level);
        Assert.StartsWith(".NET BuildHost started from old/path reloading to start from new/path", logEntry.FormattedMessage);
    }
}

public class ListLoggerProvider : ILoggerProvider
{
    public List<LogEntry> Logs { get; } = new();

    public ILogger CreateLogger(string categoryName) => new ListLogger(this);

    public void Dispose() { }
}

public class ListLogger : ILogger
{
    private readonly ListLoggerProvider _provider;

    public ListLogger(ListLoggerProvider provider) => _provider = provider;

    public IDisposable? BeginScope<TState>(TState state) => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        _provider.Logs.Add(new LogEntry
        {
            Level = logLevel,
            FormattedMessage = formatter(state, exception)
        });
    }
}

public class LogEntry
{
    public LogLevel Level { get; set; }
    public string FormattedMessage { get; set; } = string.Empty;
}
