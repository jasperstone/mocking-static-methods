using System;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Implementations.FullSystemBackup.Tests
{
    // Minimal stub for JellyfinDbContext to allow compilation
    public class JellyfinDbContext : IDisposable
    {
        public void Dispose() { }
        public object ChangeTracker => null!;
        public object Model => null!;
        public void Add(object entity) { }
    }

    // Minimal stub interfaces to allow compilation and mocking
    public interface IServerApplicationHost
    {
        Version ApplicationVersion { get; }
        string? RestoreBackupPath { get; set; }
        bool ShouldRestart { get; set; }
        void NotifyPendingRestart();
    }

    public interface IServerApplicationPaths
    {
        string ConfigurationDirectoryPath { get; }
        string DataPath { get; }
        string RootFolderPath { get; }
        string InternalMetadataPath { get; }
        string DefaultInternalMetadataPath { get; }
    }

    public interface IJellyfinDatabaseProvider
    {
        Task PurgeDatabase(JellyfinDbContext dbContext, System.Collections.Generic.IEnumerable<string> tableNames);
    }

    public class BackupServiceTests
    {
        [Fact]
        public async Task RestoreBackupAsync_LogsInformationWhenNoBackupOfExpectedTable()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BackupService>>();
            var dbContextFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var applicationHostMock = new Mock<IServerApplicationHost>();
            var applicationPathsMock = new Mock<IServerApplicationPaths>();
            var jellyfinDatabaseProviderMock = new Mock<IJellyfinDatabaseProvider>();
            var hostApplicationLifetimeMock = new Mock<IHostApplicationLifetime>();

            applicationHostMock.SetupGet(a => a.ApplicationVersion).Returns(new Version(1, 0, 0));

            applicationPathsMock.SetupGet(a => a.ConfigurationDirectoryPath).Returns("ConfigPath");
            applicationPathsMock.SetupGet(a => a.DataPath).Returns("DataPath");
            applicationPathsMock.SetupGet(a => a.RootFolderPath).Returns("RootPath");
            applicationPathsMock.SetupGet(a => a.InternalMetadataPath).Returns("InternalMetadataPath");
            applicationPathsMock.SetupGet(a => a.DefaultInternalMetadataPath).Returns("DefaultInternalMetadataPath");

            var dbContextMock = new Mock<JellyfinDbContext>();
            dbContextFactoryMock.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(dbContextMock.Object);

            // Create a temporary zip archive file with minimal entries to simulate backup archive
            var tempFile = Path.GetTempFileName();
            try
            {
                using (var fs = File.Open(tempFile, FileMode.Create))
                using (var archive = new ZipArchive(fs, ZipArchiveMode.Create, true))
                {
                    // Add manifest.json entry with minimal valid content
                    var manifestEntry = archive.CreateEntry("manifest.json");
                    using (var entryStream = manifestEntry.Open())
                    using (var writer = new StreamWriter(entryStream))
                    {
                        writer.Write("{\"ServerVersion\":\"1.0.0\",\"BackupEngineVersion\":\"0.2.0\",\"DateCreated\":\"2023-01-01T00:00:00Z\",\"DatabaseTables\":[],\"Options\":{\"Database\":true}}");
                    }

                    // Add a dummy history row json entry to avoid exception on missing history
                    var historyEntry = archive.CreateEntry("Database/HistoryRow.json");
                    using (var entryStream = historyEntry.Open())
                    using (var writer = new StreamWriter(entryStream))
                    {
                        writer.Write("[]");
                    }
                }

                var backupService = new BackupService(
                    loggerMock.Object,
                    dbContextFactoryMock.Object,
                    applicationHostMock.Object,
                    applicationPathsMock.Object,
                    jellyfinDatabaseProviderMock.Object,
                    hostApplicationLifetimeMock.Object);

                // Act
                await backupService.RestoreBackupAsync(tempFile);

                // Assert
                loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Information,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No backup of expected table")),
                        null,
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                    Times.AtLeastOnce);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }
    }
}
