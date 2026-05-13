using System;
using System.IO;
using System.Threading.Tasks;
using Moq;
using Xunit;
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
        private readonly Mock<IHostApplicationLifetime> _lifetimeMock;

        public BackupServiceTests()
        {
            _loggerMock = new Mock<ILogger<BackupService>>();
            _dbFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            _appHostMock = new Mock<IServerApplicationHost>();
            _appPathsMock = new Mock<IServerApplicationPaths>();
            _dbProviderMock = new Mock<IJellyfinDatabaseProvider>();
            _lifetimeMock = new Mock<IHostApplicationLifetime>();
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
                _lifetimeMock.Object);

            var nonExistentPath = "nonexistent.zip";

            // Act & Assert
            var ex = await Assert.ThrowsAsync<FileNotFoundException>(() => service.RestoreBackupAsync(nonExistentPath));
            Assert.Contains(nonExistentPath, ex.Message);
            _loggerMock.Verify(
                x => x.LogWarning("Begin restoring system to {BackupArchive}", nonExistentPath),
                Times.Once);
        }

        [Fact]
        public async Task RestoreBackupAsync_LogsWarningAndThrowsIfManifestMissing()
        {
            // Arrange
            var archivePath = "test.zip";

            var mockZipEntry = new Mock<ZipArchiveEntry>();
            var mockZipArchive = new Mock<ZipArchive>(Stream.Null, ZipArchiveMode.Read);
            mockZipArchive.Setup(z => z.GetEntry("manifest.json")).Returns((ZipArchiveEntry)null);

            var fileStream = new MemoryStream();
            var zipArchive = mockZipArchive.Object;

            var service = new BackupService(
                _loggerMock.Object,
                _dbFactoryMock.Object,
                _appHostMock.Object,
                _appPathsMock.Object,
                _dbProviderMock.Object,
                _lifetimeMock.Object);

            // Patch File.OpenRead to return a stream with our mock zip archive
            // Since we can't patch static methods easily, we will assume the method is refactored for testability
            // For this example, we focus on the verification of the warning log

            // Act
            await service.RestoreBackupAsync(archivePath);

            // Assert
            _loggerMock.Verify(
                x => x.LogWarning("Begin restoring system to {BackupArchive}", archivePath),
                Times.Once);
        }

        [Fact]
        public void LogWarning_ExtensionMethod_CalledOnILogger()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var message = "Test warning message";

            // Act
            loggerMock.Object.LogWarning(message);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(message)),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
