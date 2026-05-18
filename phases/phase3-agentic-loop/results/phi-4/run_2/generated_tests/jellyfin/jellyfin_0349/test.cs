using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Server.Implementations.FullSystemBackup;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Tests.FullSystemBackup
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
            zipArchiveMock.Setup(a => a.GetEntry(It.IsAny<string>())).Returns((ZipArchiveEntry)null);

            using var memoryStream = new MemoryStream();
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                archive.CreateEntry("Database/NonExistentTable.json");
            }
            memoryStream.Position = 0;
            zipArchiveMock.Setup(a => a.Entries).Returns(memoryStream.GetEntries());

            // Act
            await backupService.RestoreBackupAsync("dummyPath");

            // Assert
            loggerMock.Verify(
                l => l.LogInformation(
                    It.Is<string>(s => s.Contains("No backup of expected table")),
                    It.Is<object[]>(o => o.Length == 1 && o[0].ToString() == "NonExistentTable")),
                Times.Once);
        }
    }
}
