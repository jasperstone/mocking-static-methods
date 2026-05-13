using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Jellyfin.Server.Implementations.FullSystemBackup;
using System.Collections.Generic;

namespace Jellyfin.Tests
{
    public class BackupServiceTests
    {
        private readonly Mock<ILogger<BackupService>> _loggerMock;
        private readonly Mock<IDbContextFactory<JellyfinDbContext>> _dbFactoryMock;
        private readonly Mock<IServerApplicationHost> _appHostMock;
        private readonly Mock<IServerApplicationPaths> _appPathsMock;
        private readonly Mock<IJellyfinDatabaseProvider> _dbProviderMock;
        private readonly Mock<IHostApplicationLifetime> _hostLifetimeMock;

        public BackupServiceTests()
        {
            _loggerMock = new Mock<ILogger<BackupService>>();
            _dbFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            _appHostMock = new Mock<IServerApplicationHost>();
            _appPathsMock = new Mock<IServerApplicationPaths>();
            _dbProviderMock = new Mock<IJellyfinDatabaseProvider>();
            _hostLifetimeMock = new Mock<IHostApplicationLifetime>();
        }

        [Fact]
        public async Task RestoreBackupAsync_LogsWarningAndThrowsIfFileNotExist()
        {
            // Arrange
            var service = new BackupService(
                _loggerMock.Object,
                _dbFactoryMock.Object,
                _appHostMock.Object,
                _appPathsMock.Object,
                _dbProviderMock.Object,
                _hostLifetimeMock.Object);

            var nonExistentPath = "nonexistent.zip";

            // Act & Assert
            await Assert.ThrowsAsync<FileNotFoundException>(() => service.RestoreBackupAsync(nonExistentPath));
            _loggerMock.Verify(
                x => x.LogWarning("Begin restoring system to {BackupArchive}", nonExistentPath),
                Times.Once);
        }

        [Fact]
        public async Task RestoreBackupAsync_LogsWarningAndThrowsIfManifestMissing()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();
            try
            {
                using (var fs = File.Create(tempFile))
                {
                    // create empty zip
                }

                var service = new BackupService(
                    _loggerMock.Object,
                    _dbFactoryMock.Object,
                    _appHostMock.Object,
                    _appPathsMock.Object,
                    _dbProviderMock.Object,
                    _hostLifetimeMock.Object);

                // Act
                await Assert.ThrowsAsync<NotSupportedException>(() => service.RestoreBackupAsync(tempFile));

                // Assert
                _loggerMock.Verify(
                    x => x.LogWarning("Begin restoring system to {BackupArchive}", tempFile),
                    Times.Once);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }
    }
}
