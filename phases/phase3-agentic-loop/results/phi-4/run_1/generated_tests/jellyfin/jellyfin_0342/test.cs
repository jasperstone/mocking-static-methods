using Moq;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Jellyfin.Server.Implementations.FullSystemBackup.Tests
{
    public class BackupServiceTests
    {
        [Fact]
        public async Task RestoreBackupAsync_LogsWarningWhenRestoring()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BackupService>>();
            var dbProviderMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var applicationHostMock = new Mock<IServerApplicationHost>();
            var applicationPathsMock = new Mock<IServerApplicationPaths>();
            var jellyfinDatabaseProviderMock = new Mock<IJellyfinDatabaseProvider>();
            var applicationLifetimeMock = new Mock<IHostApplicationLifetime>();

            var backupService = new BackupService(
                loggerMock.Object,
                dbProviderMock.Object,
                applicationHostMock.Object,
                applicationPathsMock.Object,
                jellyfinDatabaseProviderMock.Object,
                applicationLifetimeMock.Object);

            string archivePath = "testBackup.zip";

            // Act
            await backupService.RestoreBackupAsync(archivePath);

            // Assert
            loggerMock.Verify(
                x => x.LogWarning(
                    It.Is<LogLevel>(l => l == LogLevel.Warning),
                    It.Is<EventId>(e => e.Id == 0), // Assuming EventId is 0, adjust if known
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Begin restoring system to {BackupArchive}")),
                    It.Is<Exception>(ex => ex == null),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);

            loggerMock.Verify(
                x => x.LogWarning(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(archivePath)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
