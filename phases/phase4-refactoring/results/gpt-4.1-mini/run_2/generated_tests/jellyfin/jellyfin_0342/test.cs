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
    public class BackupServiceTests
    {
        private class DummyDbContextFactory : IDbContextFactory<object>
        {
            public Task<object> CreateDbContextAsync()
            {
                return Task.FromResult<object>(null);
            }
        }

        private class DummyJellyfinDatabaseProvider : IJellyfinDatabaseProvider
        {
        }

        [Fact]
        public async Task RestoreBackupAsync_LogsWarningWithArchivePath()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BackupService>>();
            var dbProvider = new DummyDbContextFactory();
            var applicationHostMock = new Mock<IServerApplicationHost>();
            var applicationPathsMock = new Mock<IServerApplicationPaths>();
            var jellyfinDatabaseProvider = new DummyJellyfinDatabaseProvider();
            var hostApplicationLifetimeMock = new Mock<IHostApplicationLifetime>();

            var backupService = new BackupService(
                loggerMock.Object,
                dbProvider,
                applicationHostMock.Object,
                applicationPathsMock.Object,
                jellyfinDatabaseProvider,
                hostApplicationLifetimeMock.Object);

            // We need to create a temporary empty file to pass the File.Exists check
            var tempFilePath = Path.GetTempFileName();

            try
            {
                // Act & Assert
                // We expect a NotSupportedException because the temp file is not a valid backup archive
                await Assert.ThrowsAsync<NotSupportedException>(() => backupService.RestoreBackupAsync(tempFilePath));

                // Verify that LogWarning was called with the expected message and argument
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
                File.Delete(tempFilePath);
            }
        }
    }
}
