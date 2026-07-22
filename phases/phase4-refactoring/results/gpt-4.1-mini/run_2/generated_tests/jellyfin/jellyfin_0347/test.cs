using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Jellyfin.Database.Implementations;
using Jellyfin.Server.Implementations.FullSystemBackup;
using MediaBrowser.Controller;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Implementations.Tests.FullSystemBackup
{
    public class BackupServiceLoggerTests
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

            applicationHostMock.SetupGet(x => x.ApplicationVersion).Returns(new Version(1, 0, 0));
            dbContextFactoryMock.Setup(x => x.CreateDbContextAsync(default)).ReturnsAsync(dbContextMock.Object);

            var databaseFacadeMock = new Mock<DatabaseFacade>(dbContextMock.Object);
            databaseFacadeMock.Setup(d => d.ExecuteSqlRawAsync(It.IsAny<string>(), default)).ReturnsAsync(1);
            dbContextMock.SetupGet(d => d.Database).Returns(databaseFacadeMock.Object);

            jellyfinDatabaseProviderMock.Setup(x => x.PurgeDatabase(It.IsAny<JellyfinDbContext>(), It.IsAny<IEnumerable<string>>())).Returns(Task.CompletedTask);

            // Create a minimal valid backup manifest JSON
            var manifestJson = JsonSerializer.Serialize(new
            {
                ServerVersion = "1.0.0",
                BackupEngineVersion = "0.2.0",
                Options = new { Database = true }
            });

            // Create a temporary zip file with manifest.json entry
            var tempZipPath = Path.GetTempFileName();
            try
            {
                using (var fs = File.Open(tempZipPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
                using (var archive = new ZipArchive(fs, ZipArchiveMode.Create, true))
                {
                    var manifestEntry = archive.CreateEntry("manifest.json");
                    using var entryStream = manifestEntry.Open();
                    var bytes = Encoding.UTF8.GetBytes(manifestJson);
                    entryStream.Write(bytes, 0, bytes.Length);
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
                loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Information,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Database Purged")),
                        null,
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.AtLeastOnce);
            }
            finally
            {
                if (File.Exists(tempZipPath))
                    File.Delete(tempZipPath);
            }
        }
    }
}
