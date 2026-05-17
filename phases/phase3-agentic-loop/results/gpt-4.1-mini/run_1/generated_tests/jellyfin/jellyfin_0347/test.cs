using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.Server.Implementations.FullSystemBackup;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Hosting;
using System.IO.Compression;
using System.Text.Json;
using System.IO;
using MediaBrowser.Controller;
using Jellyfin.Database.Implementations;

namespace Jellyfin.Server.Implementations.FullSystemBackup.Tests
{
    public class BackupServiceTests
    {
        [Fact]
        public async Task RestoreBackupAsync_LogsDatabasePurged()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BackupService>>();
            var dbContextFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var applicationHostMock = new Mock<IServerApplicationHost>();
            var applicationPathsMock = new Mock<IServerApplicationPaths>();
            var jellyfinDatabaseProviderMock = new Mock<IJellyfinDatabaseProvider>();
            var hostApplicationLifetimeMock = new Mock<IHostApplicationLifetime>();

            // Setup a DbContext mock with minimal required behavior
            var dbContextMock = new Mock<JellyfinDbContext>();
            var databaseFacadeMock = new Mock<DatabaseFacade>(dbContextMock.Object);
            dbContextMock.SetupGet(d => d.Database).Returns(databaseFacadeMock.Object);
            var modelMock = new Mock<Microsoft.EntityFrameworkCore.Metadata.IModel>();
            dbContextMock.SetupGet(d => d.Model).Returns(modelMock.Object);

            dbContextFactoryMock.Setup(f => f.CreateDbContextAsync(It.IsAny<System.Threading.CancellationToken>())).ReturnsAsync(dbContextMock.Object);

            // Setup jellyfinDatabaseProvider to simulate PurgeDatabase call
            jellyfinDatabaseProviderMock.Setup(j => j.PurgeDatabase(It.IsAny<JellyfinDbContext>(), It.IsAny<IEnumerable<string>>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Setup applicationHost version and other properties
            applicationHostMock.SetupGet(a => a.ApplicationVersion).Returns(new Version(1, 0, 0));

            // Setup a minimal BackupManifest with database option true
            var manifest = new BackupManifest
            {
                ServerVersion = new Version(1, 0, 0),
                BackupEngineVersion = new Version(0, 2, 0),
                Options = new BackupOptions { Database = true }
            };

            // We need to simulate the zip archive and entries for the RestoreBackupAsync method
            // Since the method reads from a file, we will create a temporary zip file with the required entries

            var tempZipPath = Path.GetTempFileName();
            try
            {
                using (var zipToCreate = ZipFile.Open(tempZipPath, ZipArchiveMode.Update))
                {
                    // Add manifest.json entry with serialized manifest
                    var manifestEntry = zipToCreate.CreateEntry("manifest.json");
                    using (var entryStream = manifestEntry.Open())
                    {
                        await JsonSerializer.SerializeAsync(entryStream, manifest);
                    }

                    // Add HistoryRow.json entry to simulate history table backup
                    var historyEntry = zipToCreate.CreateEntry("Database/HistoryRow.json");
                    using (var entryStream = historyEntry.Open())
                    using (var writer = new StreamWriter(entryStream))
                    {
                        await writer.WriteAsync("[]");
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
                await backupService.RestoreBackupAsync(tempZipPath);

                // Assert
                // Verify that the logger was called with "Database Purged"
                loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Information,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Database Purged")),
                        null,
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                    Times.AtLeastOnce);

                jellyfinDatabaseProviderMock.Verify(j => j.PurgeDatabase(It.IsAny<JellyfinDbContext>(), It.IsAny<IEnumerable<string>>()), Times.Once);
            }
            finally
            {
                if (File.Exists(tempZipPath))
                {
                    File.Delete(tempZipPath);
                }
            }
        }
    }
}
