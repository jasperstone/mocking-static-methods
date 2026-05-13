using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.Server.Implementations.FullSystemBackup;
using Microsoft.Extensions.Hosting;
using MediaBrowser.Controller;

namespace Jellyfin.Server.Implementations.FullSystemBackup.Tests
{
    public class BackupServiceTests
    {
        [Fact]
        public async Task RestoreBackupAsync_LogsWarningWithArchivePath()
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

            // Create a temporary empty file to simulate the archive file
            var tempFile = Path.GetTempFileName();

            try
            {
                // Setup File.Exists to return true for the temp file path
                // We cannot mock static File.Exists easily, so we create the file physically

                // Setup applicationHost.ApplicationVersion to a version that will not throw
                applicationHostMock.SetupGet(x => x.ApplicationVersion).Returns(new Version(1, 0, 0));

                // Setup StorageHelper.TestCommonPathsForStorageCapacity to do nothing
                // This is a static method, so we cannot mock it here, but it should not affect logging

                // Setup dbProvider.CreateDbContextAsync to return a mock DbContext
                var dbContextMock = new Mock<JellyfinDbContext>();
                dbProviderMock.Setup(x => x.CreateDbContextAsync(default)).ReturnsAsync(dbContextMock.Object);

                // Setup zip archive entries to include the manifest entry with a valid manifest
                // We cannot easily mock ZipArchive or File.OpenRead, so we will just test the logging call before file checks

                // Act
                // We expect the LogWarning call to happen before any file checks, so we can call RestoreBackupAsync and catch exceptions
                await Assert.ThrowsAnyAsync<Exception>(() => backupService.RestoreBackupAsync(tempFile));

                // Assert
                loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Warning,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Begin restoring system to") && v.ToString().Contains(tempFile)),
                        null,
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.Once);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }
    }
}
