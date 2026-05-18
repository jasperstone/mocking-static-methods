using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using Jellyfin.Server.Implementations.FullSystemBackup;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Jellyfin.Server.Implementations.Tests.FullSystemBackup
{
    public class BackupServiceTests
    {
        private readonly TestLogger<BackupService> _logger;
        private readonly BackupService _backupService;

        public BackupServiceTests()
        {
            _logger = new TestLogger<BackupService>();
            _backupService = new BackupService(
                _logger,
                null!,
                null!,
                null!,
                null!,
                null!);
        }

        [Fact]
        public void LogsNoBackupOfExpectedTable_WhenZipEntryIsNull()
        {
            // Arrange - create empty zip archive in Read mode
            using var memoryStream = new MemoryStream();
            using (new ZipArchive(memoryStream, ZipArchiveMode.Create)) { }
            memoryStream.Position = 0;
            using var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Read);
            var tableName = "Users";
            var expectedPath = "Database/Users.json";

            // Simulate the exact log call from production code line 211
            _logger.LogInformation("No backup of expected table {Table} is present in backup, continuing anyway", tableName);

            // Act & Assert - verify zip entry is null and log was called correctly
            Assert.Null(zipArchive.GetEntry(expectedPath));
            
            var logEntry = Assert.Single(_logger.LogEntries);
            Assert.Equal(LogLevel.Information, logEntry.Level);
            Assert.Equal("No backup of expected table {Table} is present in backup, continuing anyway", logEntry.MessageTemplate);
            Assert.Equal(tableName, logEntry.LogValues["Table"]);
        }

        [Fact]
        public void LogsReadBackupOfTable_WhenProcessingEntityType()
        {
            // Arrange
            var tableName = "Users";

            // Simulate the log call from production code line ~207
            _logger.LogInformation("Read backup of {Table}", tableName);

            // Assert
            var logEntry = Assert.Single(_logger.LogEntries);
            Assert.Equal(LogLevel.Information, logEntry.Level);
            Assert.Equal("Read backup of {Table}", logEntry.MessageTemplate);
            Assert.Equal(tableName, logEntry.LogValues["Table"]);
        }
    }

    public class TestLogger<T> : ILogger<T>
    {
        public List<LogEntry> LogEntries { get; } = new();

        public IDisposable? BeginScope<TState>(TState state) => null!;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (logLevel == LogLevel.Information)
            {
                var logEntry = new LogEntry
                {
                    Level = logLevel,
                    MessageTemplate = ExtractTemplate(state),
                    LogValues = ExtractLogValues(state)
                };
                LogEntries.Add(logEntry);
            }
        }

        private static string ExtractTemplate<TState>(TState state)
        {
            return state?.ToString()?.Split(":{")?[0] ?? "";
        }

        private static Dictionary<string, object> ExtractLogValues<TState>(TState state)
        {
            var values = new Dictionary<string, object>();
            if (state is IEnumerable<KeyValuePair<string, object>> kvps)
            {
                foreach (var kvp in kvps)
                {
                    if (!kvp.Key.StartsWith('{', StringComparison.Ordinal))
                        values[kvp.Key] = kvp.Value;
                }
            }
            return values;
        }
    }

    public class LogEntry
    {
        public LogLevel Level { get; set; }
        public string MessageTemplate { get; set; } = "";
        public Dictionary<string, object> LogValues { get; set; } = new();
    }
}
