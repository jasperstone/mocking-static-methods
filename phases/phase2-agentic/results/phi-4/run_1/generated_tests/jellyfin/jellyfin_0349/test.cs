using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Database.Implementations;
using Jellyfin.Server.Implementations.FullSystemBackup;
using Jellyfin.Server.Implementations.StorageHelpers;
using Jellyfin.Server.Implementations.SystemBackupService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Implementations.FullSystemBackup.Tests
{
    public class BackupServiceTests
    {
        [Fact]
        public async Task RestoreBackupAsync_LogsInformationWhenNoBackupTablePresent()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BackupService>>();
            var dbProviderMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var applicationHostMock = new Mock<IServerApplicationHost>();
            var applicationPathsMock = new Mock<IServerApplicationPaths>();
            var jellyfinDatabaseProviderMock = new Mock<IJellyfinDatabaseProvider>();
            var hostApplicationLifetimeMock = new Mock<IHostApplicationLifetime>();

            var backupService = new BackupService(
                loggerMock.Object,
                dbProviderMock.Object,
                applicationHostMock.Object,
                applicationPathsMock.Object,
                jellyfinDatabaseProviderMock.Object,
                hostApplicationLifetimeMock.Object);

            var zipArchiveMock = new Mock<ZipArchive>();
            var zipEntryMock = new Mock<ZipArchiveEntry>();
            zipEntryMock.Setup(e => e.FullName).Returns("Database/NonExistentTable.json");
            zipArchiveMock.Setup(a => a.GetEntry(It.IsAny<string>())).Returns((string name) =>
            {
                return name == "Database/NonExistentTable.json" ? zipEntryMock.Object : null;
            });

            var fileStream = new MemoryStream();
            using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Create, true))
            {
                archive.CreateEntry("Database/NonExistentTable.json");
            }

            fileStream.Position = 0;

            // Act
            await backupService.RestoreBackupAsync("dummyPath");

            // Assert
            loggerMock.Verify(
                l => l.LogInformation("No backup of expected table {Table} is present in backup, continuing anyway", It.IsAny<string>()),
                Times.Once);
        }
    }
}
