using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Jellyfin.Server.Implementations.FullSystemBackup;
using Microsoft.EntityFrameworkCore;
using MediaBrowser.Controller;
using MediaBrowser.Controller.SystemBackupService;
using Jellyfin.Database.Implementations;
using Jellyfin.Server.Implementations.StorageHelpers;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading;
using Microsoft.Extensions.Hosting;

namespace Jellyfin.Server.Tests
{
    public class BackupServiceTests
    {
        [Fact]
        public async Task RestoreBackupAsync_LogsInformation()
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
            var archivePath = Path.GetTempFileName();
            await using (var stream = File.Create(archivePath))
            {
                await backupService.RestoreBackupAsync(archivePath);
            }
            File.Delete(archivePath);

            // Assert
            loggerMock.Verify(logger => logger.LogInformation("Begin purging database"), Times.Once);
            loggerMock.Verify(logger => logger.LogInformation("Database Purged"), Times.Once);
        }
    }
}
