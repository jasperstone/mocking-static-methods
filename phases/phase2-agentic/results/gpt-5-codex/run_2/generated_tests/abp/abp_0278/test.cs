using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands;

public class SuiteCommandTests
{
    [Fact]
    public void ShowSuiteManualUpdateCommand_Should_Log_Manual_Update_Instructions()
    {
        var suiteCommand = new SuiteCommand(null, null, null, null, null, null);
        var testLogger = new TestLogger<SuiteCommand>();
        suiteCommand.Logger = testLogger;

        var method = typeof(SuiteCommand).GetMethod(
            "ShowSuiteManualUpdateCommand",
            BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.NotNull(method);

        method!.Invoke(suiteCommand, Array.Empty<object>());

        Assert.Equal(2, testLogger.Logs.Count);
        Assert.Contains(testLogger.Logs, entry =>
            entry.LogLevel == LogLevel.Error &&
            entry.Message == "You can also run the following command to update ABP Suite.");
        Assert.Contains(testLogger.Logs, entry =>
            entry.LogLevel == LogLevel.Error &&
            entry.Message == "dotnet tool update -g Volo.Abp.Suite --add-source https://nuget.abp.io/<your-private-key>/v3/index.json");
    }

    private sealed class TestLogger<T> : ILogger<T>
    {
        public List<LogEntry> Logs { get; } = new();

        public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            var message = formatter != null ? formatter(state, exception) : state?.ToString() ?? string.Empty;
            Logs.Add(new LogEntry(logLevel, eventId, message, exception));
        }

        public sealed record LogEntry(LogLevel LogLevel, EventId EventId, string Message, Exception? Exception);

        private sealed class NullScope : IDisposable
        {
            public static NullScope Instance { get; } = new();

            public void Dispose()
            {
            }
        }
    }
}
