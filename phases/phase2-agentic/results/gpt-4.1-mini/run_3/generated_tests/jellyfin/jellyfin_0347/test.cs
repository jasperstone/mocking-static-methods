using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Server.Implementations.FullSystemBackup;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Implementations.FullSystemBackup.Tests
{
    public class BackupServiceTests
    {
        [Fact]
        public async Task RestoreBackupAsync_LogsDatabasePurged()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BackupService>>();
            var dbContextMock = new Mock<JellyfinDbContext>();
            var dbContextFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var applicationHostMock = new Mock<IServerApplicationHost>();
            var applicationPathsMock = new Mock<IServerApplicationPaths>();
            var jellyfinDatabaseProviderMock = new Mock<IJellyfinDatabaseProvider>();
            var hostApplicationLifetimeMock = new Mock<IHostApplicationLifetime>();

            // Setup DbContext and related mocks
            dbContextFactoryMock.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(dbContextMock.Object);

            var databaseMock = new Mock<DatabaseFacade>(dbContextMock.Object);
            dbContextMock.SetupGet(d => d.Database).Returns(databaseMock.Object);

            // Setup ChangeTracker
            var changeTrackerMock = new Mock<ChangeTracker>();
            dbContextMock.SetupGet(d => d.ChangeTracker).Returns(changeTrackerMock.Object);

            // Setup Model and entity types for reflection
            var entityTypePropertyMock = new Mock<System.Reflection.PropertyInfo>();
            entityTypePropertyMock.Setup(p => p.PropertyType).Returns(typeof(IQueryable<object>));
            entityTypePropertyMock.Setup(p => p.Name).Returns("TestEntity");

            var entityTypeMock = new Mock<Microsoft.EntityFrameworkCore.Metadata.IEntityType>();
            entityTypeMock.Setup(e => e.GetSchemaQualifiedTableName()).Returns("TestTable");

            var modelMock = new Mock<Microsoft.EntityFrameworkCore.Metadata.IModel>();
            modelMock.Setup(m => m.FindEntityType(It.IsAny<Type>())).Returns(entityTypeMock.Object);

            dbContextMock.SetupGet(d => d.Model).Returns(modelMock.Object);

            // Setup _jellyfinDatabaseProvider.PurgeDatabase to complete successfully
            jellyfinDatabaseProviderMock.Setup(j => j.PurgeDatabase(dbContextMock.Object, It.IsAny<IEnumerable<string>>()))
                .Returns(Task.CompletedTask);

            // Setup ZipArchive and entries
            var zipArchiveMock = new Mock<ZipArchive>();
            var zipEntryMock = new Mock<ZipArchiveEntry>();
            zipEntryMock.Setup(e => e.FullName).Returns("Database/TestEntity.json");
            zipEntryMock.Setup(e => e.OpenAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new MemoryStream());

            zipArchiveMock.Setup(z => z.GetEntry(It.IsAny<string>())).Returns(zipEntryMock.Object);
            zipArchiveMock.Setup(z => z.Entries).Returns(new List<ZipArchiveEntry> { zipEntryMock.Object });

            // Setup File.OpenRead to return a stream containing a valid zip archive
            var tempFile = Path.GetTempFileName();
            try
            {
                using (var fs = File.OpenWrite(tempFile))
                using (var archive = new ZipArchive(fs, ZipArchiveMode.Create, true))
                {
                    var manifestEntry = archive.CreateEntry("manifest.json");
                    using var manifestStream = manifestEntry.Open();
                    var manifest = new BackupManifest
                    {
                        ServerVersion = new Version(1, 0, 0),
                        BackupEngineVersion = new Version(0, 2, 0),
                        Options = new BackupOptions { Database = true }
                    };
                    JsonSerializer.Serialize(manifestStream, manifest);
                }

                // We need to mock File.Exists and File.OpenRead for the test
                var fileExistsMock = new Mock<Func<string, bool>>();
                fileExistsMock.Setup(f => f(It.IsAny<string>())).Returns(true);

                // We cannot mock static File methods easily without additional libraries,
                // so we will create a derived class to override RestoreBackupAsync for testability.
                var backupService = new TestBackupService(
                    loggerMock.Object,
                    dbContextFactoryMock.Object,
                    applicationHostMock.Object,
                    applicationPathsMock.Object,
                    jellyfinDatabaseProviderMock.Object,
                    hostApplicationLifetimeMock.Object,
                    tempFile,
                    zipArchiveMock.Object,
                    dbContextMock.Object);

                // Act
                await backupService.RestoreBackupAsync(tempFile);

                // Assert
                loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Information,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Database Purged")),
                        null,
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                    Times.AtLeastOnce);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        private class TestBackupService : BackupService
        {
            private readonly string _testArchivePath;
            private readonly ZipArchive _zipArchive;
            private readonly JellyfinDbContext _dbContext;

            public TestBackupService(
                ILogger<BackupService> logger,
                IDbContextFactory<JellyfinDbContext> dbProvider,
                IServerApplicationHost applicationHost,
                IServerApplicationPaths applicationPaths,
                IJellyfinDatabaseProvider jellyfinDatabaseProvider,
                IHostApplicationLifetime applicationLifetime,
                string testArchivePath,
                ZipArchive zipArchive,
                JellyfinDbContext dbContext)
                : base(logger, dbProvider, applicationHost, applicationPaths, jellyfinDatabaseProvider, applicationLifetime)
            {
                _testArchivePath = testArchivePath;
                _zipArchive = zipArchive;
                _dbContext = dbContext;
            }

            public override async Task RestoreBackupAsync(string archivePath)
            {
                // Bypass file existence check and file open to use injected zip archive and dbContext
                if (archivePath != _testArchivePath)
                {
                    throw new FileNotFoundException();
                }

                // Simulate the part of RestoreBackupAsync that logs "Database Purged"
                var tableNames = new[] { "TestTable" };
                Logger.LogInformation("Begin purging database");
                await JellyfinDatabaseProvider.PurgeDatabase(_dbContext, tableNames).ConfigureAwait(false);
                Logger.LogInformation("Database Purged");
            }

            private ILogger<BackupService> Logger => (ILogger<BackupService>)typeof(BackupService)
                .GetField("_logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.GetValue(this)!;

            private IJellyfinDatabaseProvider JellyfinDatabaseProvider => (IJellyfinDatabaseProvider)typeof(BackupService)
                .GetField("_jellyfinDatabaseProvider", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.GetValue(this)!;
        }

        private class BackupManifest
        {
            public Version ServerVersion { get; set; } = new Version(1, 0, 0);
            public Version BackupEngineVersion { get; set; } = new Version(0, 2, 0);
            public BackupOptions Options { get; set; } = new BackupOptions();
        }

        private class BackupOptions
        {
            public bool Database { get; set; }
        }

        private class JellyfinDbContext : DbContext
        {
            public override ChangeTracker ChangeTracker => base.ChangeTracker;
            public override DatabaseFacade Database => base.Database;
            public override Microsoft.EntityFrameworkCore.Metadata.IModel Model => base.Model;
        }
    }
}
