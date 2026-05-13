using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Jellyfin.Server.Implementations.FullSystemBackup;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Implementations.FullSystemBackup.Tests
{
    public class BackupServiceTests
    {
        [Fact]
        public async Task RestoreBackupAsync_LogsInformationOnMissingBackupEntry()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BackupService>>();
            var dbContextFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var applicationHostMock = new Mock<IServerApplicationHost>();
            var applicationPathsMock = new Mock<IServerApplicationPaths>();
            var jellyfinDatabaseProviderMock = new Mock<IJellyfinDatabaseProvider>();
            var hostApplicationLifetimeMock = new Mock<Microsoft.Extensions.Hosting.IHostApplicationLifetime>();

            // Setup application version to be compatible
            applicationHostMock.SetupGet(a => a.ApplicationVersion).Returns(new Version(1, 0, 0));

            // Setup dbContext and its behavior
            var dbContextMock = new Mock<JellyfinDbContext>();
            dbContextMock.SetupGet(d => d.ChangeTracker).Returns(Mock.Of<ChangeTracker>());
            dbContextMock.SetupGet(d => d.Model).Returns(Mock.Of<IModel>());

            dbContextFactoryMock.Setup(f => f.CreateDbContextAsync(default))
                .ReturnsAsync(dbContextMock.Object);

            // Setup jellyfinDatabaseProvider to do nothing on PurgeDatabase
            jellyfinDatabaseProviderMock.Setup(j => j.PurgeDatabase(It.IsAny<JellyfinDbContext>(), It.IsAny<IEnumerable<string>>()))
                .Returns(Task.CompletedTask);

            // Create a ZipArchive with entries for manifest and history but missing one table backup entry
            var memoryStream = new MemoryStream();
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                // Add manifest.json entry
                var manifestEntry = archive.CreateEntry("manifest.json");
                using (var writer = new StreamWriter(manifestEntry.Open()))
                {
                    var manifest = new BackupManifest
                    {
                        ServerVersion = new Version(1, 0, 0),
                        BackupEngineVersion = new Version(0, 2, 0),
                        Options = new BackupOptions { Database = true }
                    };
                    writer.Write(JsonSerializer.Serialize(manifest));
                }

                // Add history row backup entry
                var historyEntry = archive.CreateEntry("Database/HistoryRow.json");
                using (var writer = new StreamWriter(historyEntry.Open()))
                {
                    writer.Write("[]");
                }

                // Add one entity type backup entry (simulate one table)
                var tableEntry = archive.CreateEntry("Database/FakeEntity.json");
                using (var writer = new StreamWriter(tableEntry.Open()))
                {
                    writer.Write("[]");
                }
            }
            memoryStream.Position = 0;

            // Setup File.Exists and File.OpenRead to use the memory stream
            var archivePath = "fakepath.zip";
            var fileExistsMock = new Mock<IFileWrapper>();
            fileExistsMock.Setup(f => f.Exists(archivePath)).Returns(true);
            fileExistsMock.Setup(f => f.OpenRead(archivePath)).Returns(memoryStream);

            // Setup BackupService with a derived class to override file system calls
            var backupService = new TestBackupService(
                loggerMock.Object,
                dbContextFactoryMock.Object,
                applicationHostMock.Object,
                applicationPathsMock.Object,
                jellyfinDatabaseProviderMock.Object,
                hostApplicationLifetimeMock.Object,
                fileExistsMock.Object);

            // Act
            await backupService.RestoreBackupAsync(archivePath);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Begin purging database")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No backup of expected table FakeEntity is present in backup, continuing anyway")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        // Helper classes and interfaces to mock file system calls
        public interface IFileWrapper
        {
            bool Exists(string path);
            Stream OpenRead(string path);
        }

        private class TestBackupService : BackupService
        {
            private readonly IFileWrapper _fileWrapper;

            public TestBackupService(
                ILogger<BackupService> logger,
                IDbContextFactory<JellyfinDbContext> dbProvider,
                IServerApplicationHost applicationHost,
                IServerApplicationPaths applicationPaths,
                IJellyfinDatabaseProvider jellyfinDatabaseProvider,
                Microsoft.Extensions.Hosting.IHostApplicationLifetime hostApplicationLifetime,
                IFileWrapper fileWrapper)
                : base(logger, dbProvider, applicationHost, applicationPaths, jellyfinDatabaseProvider, hostApplicationLifetime)
            {
                _fileWrapper = fileWrapper;
            }

            protected override bool FileExists(string path) => _fileWrapper.Exists(path);

            protected override Stream OpenFileRead(string path) => _fileWrapper.OpenRead(path);
        }
    }

    // Minimal stubs for types used in BackupService
    public class BackupManifest
    {
        public Version ServerVersion { get; set; } = new Version(1, 0, 0);
        public Version BackupEngineVersion { get; set; } = new Version(0, 2, 0);
        public BackupOptions Options { get; set; } = new BackupOptions();
    }

    public class BackupOptions
    {
        public bool Database { get; set; }
    }

    public class JellyfinDbContext : DbContext
    {
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
}
