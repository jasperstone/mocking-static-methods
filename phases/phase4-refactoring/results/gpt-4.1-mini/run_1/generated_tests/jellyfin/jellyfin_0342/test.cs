using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.Extensions.Hosting;
using MediaBrowser.Controller;
using Jellyfin.Server.Implementations.FullSystemBackup;

namespace Jellyfin.Server.Implementations.FullSystemBackup.Tests
{
    // Minimal interface definitions to satisfy dependencies for testing
    public interface IDbContextFactory<T>
    {
        Task<T> CreateDbContextAsync();
    }

    public interface IJellyfinDatabaseProvider
    {
    }

    public class BackupServiceTests
    {
        [Fact]
        public async Task RestoreBackupAsync_LogsWarningWithArchivePath()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BackupService>>();
            var dbProviderMock = new Mock<IDbContextFactory<object>>();
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

            var tempFile = Path.GetTempFileName();
            try
            {
                // Act & Assert
                // We expect a FileNotFoundException because the temp file is empty and not a valid backup,
                // but the log warning should be called before that.
                await Assert.ThrowsAsync<FileNotFoundException>(() => backupService.RestoreBackupAsync(tempFile));

                // Verify the LogWarning call with the expected message and argument
                loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Warning,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Begin restoring system to")),
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
