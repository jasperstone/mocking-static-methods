using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Xunit;
using Volo.Abp.Cli.ProjectModification;

namespace Volo.Abp.Cli.ProjectModification.Tests;

public class LocalReferenceConverterTests
{
    [Fact]
    public async Task ConvertAsync_Should_Log_Initial_Message()
    {
        var testLogger = new TestLogger<LocalReferenceConverter>();
        var converter = new LocalReferenceConverter
        {
            Logger = testLogger
        };

        var tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDirectory);

        try
        {
            await converter.ConvertAsync(tempDirectory, new List<string>());

            var matchCount = 0;
            foreach (var entry in testLogger.LogEntries)
            {
                if (entry.Level == LogLevel.Information &&
                    entry.Message == "Converting projects to local reference.")
                {
                    matchCount++;
                }
            }

            Assert.Equal(1, matchCount);
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, true);
            }
        }
    }

    private sealed class TestLogger<T> : ILogger<T>
    {
        private sealed class NullScope : IDisposable
        {
            public static NullScope Instance { get; } = new NullScope();
            public void Dispose()
            {
            }
        }

        public IList<LogEntry> LogEntries { get; } = new List<LogEntry>();

        public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
            Func<TState, Exception, string> formatter)
        {
            var message = formatter != null
                ? formatter(state, exception)
                : state?.ToString() ?? string.Empty;

            LogEntries.Add(new LogEntry(logLevel, message));
        }

        public sealed record LogEntry(LogLevel Level, string Message);
    }
}
