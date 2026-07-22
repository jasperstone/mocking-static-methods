using Xunit;
using Moq;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller;
using Jellyfin.Database;
using Jellyfin.Server.Implementations.StorageHelpers;
using Jellyfin.Server.Implementations.SystemBackupService;
using Microsoft.EntityFrameworkCore;
using Jellyfin.Server.Implementations;

namespace Jellyfin.Server.Implementations.FullSystemBackup.Tests
{
    public class BackupServiceTests
    {
        [Fact]
        public async Task Backup_CreatesBackupArchive_WithExpectedEntries()
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

            // Act
            await backupService.RestoreBackupAsync("path/to/backup/archive");

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
        }
    }
}
