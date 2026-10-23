using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Implementations.FullSystemBackup.Tests
{
    // Minimal stubs for missing types to allow compilation and mocking
    public class JellyfinDbContext : DbContext
    {
        public override DatabaseFacade Database => base.Database;
        public override Microsoft.EntityFrameworkCore.Metadata.IModel Model => base.Model;
    }

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
        Task PurgeDatabase(JellyfinDbContext dbContext, IEnumerable<string> tableNames);
    }

    public class BackupManifest
    {
        public Version ServerVersion { get; set; } = new Version(1, 0, 0);
        public Version BackupEngineVersion { get; set; } = new Version(0, 2, 0);
        public BackupOptions Options { get; set; } = new BackupOptions();
    }

    public class BackupOptions
    {
        public bool Database { get; set; } = true;
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

            var dbContextMock = new Mock<JellyfinDbContext>();
            dbContextFactoryMock.Setup(f => f.CreateDbContextAsync(default))
                .ReturnsAsync(dbContextMock.Object);

            applicationHostMock.SetupGet(a => a.ApplicationVersion).Returns(new Version(1, 0, 0));

            applicationPathsMock.SetupGet(a => a.ConfigurationDirectoryPath).Returns(Path.GetTempPath());
            applicationPathsMock.SetupGet(a => a.DataPath).Returns(Path.GetTempPath());
            applicationPathsMock.SetupGet(a => a.RootFolderPath).Returns(Path.GetTempPath());
            applicationPathsMock.SetupGet(a => a.InternalMetadataPath).Returns(Path.GetTempPath());
            applicationPathsMock.SetupGet(a => a.DefaultInternalMetadataPath).Returns(Path.GetTempPath());

            jellyfinDatabaseProviderMock.Setup(j => j.PurgeDatabase(It.IsAny<JellyfinDbContext>(), It.IsAny<IEnumerable<string>>()))
                .Returns(Task.CompletedTask);

            // Create a temporary zip archive with minimal entries to trigger the log line
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
                        writer.Write("{\"ServerVersion\":\"1.0.0\",\"BackupEngineVersion\":\"0.2.0\",\"Options\":{\"Database\":true}}");
                    }

                    // Add a dummy history row json entry to avoid exception
                    var historyEntry = archive.CreateEntry("Database/HistoryRow.json");
                    using (var entryStream = historyEntry.Open())
                    using (var writer = new StreamWriter(entryStream))
                    {
                        writer.Write("[]");
                    }

                    // Add no entry for SomeEntity.json to trigger the log line on line 211
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
