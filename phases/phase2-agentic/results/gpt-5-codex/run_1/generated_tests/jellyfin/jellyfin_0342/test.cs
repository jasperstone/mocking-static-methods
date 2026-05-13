using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Jellyfin.Server.Implementations.FullSystemBackup;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Jellyfin.Server.Implementations.Tests.FullSystemBackup;

public class BackupServiceLoggingTests
{
    [Fact]
    public async Task RestoreBackupAsync_LogsWarning_WhenBackupFileIsMissing()
    {
        var logger = new TestLogger<BackupService>();
        var backupService = new BackupService(logger, null!, null!, null!, null!, null!);
        var archivePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.zip");

        await Assert.ThrowsAsync<FileNotFoundException>(() => backupService.RestoreBackupAsync(archivePath));

        var logEntry = Assert.Single(logger.Entries);
        Assert.Equal(LogLevel.Warning, logEntry.LogLevel);
        Assert.Equal($"Begin restoring system to {archivePath}", logEntry.Message);

        var state = Assert.IsAssignableFrom<IReadOnlyList<KeyValuePair<string, object>>>(logEntry.State!);
        var backupArchiveProperty = state.Single(kvp => kvp.Key == "BackupArchive");
        Assert.Equal(archivePath, backupArchiveProperty.Value?.ToString());

        var originalFormatProperty = state.Single(kvp => kvp.Key == "{OriginalFormat}");
        Assert.Equal("Begin restoring system to {BackupArchive}", originalFormatProperty.Value);
    }

    private sealed class TestLogger<T> : ILogger<T>
    {
        public List<LogEntry> Entries { get; } = new();

        public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            Entries.Add(new LogEntry(logLevel, eventId, state, exception, formatter(state, exception)));
        }

        internal sealed record LogEntry(LogLevel LogLevel, EventId EventId, object? State, Exception? Exception, string Message);

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();

            public void Dispose()
            {
            }
        }
    }
}
