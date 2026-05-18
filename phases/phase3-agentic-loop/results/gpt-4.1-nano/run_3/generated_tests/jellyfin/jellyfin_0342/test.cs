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
            _loggerMock.Verify(x => x.LogWarning(It.IsAny<string>(), nonExistentPath), Times.Once);
        }

        [Fact]
        public async Task RestoreBackupAsync_LogsWarningAndCallsStopApplication()
        {
            // Arrange
            var archivePath = "test.zip";

            var fileStreamMock = new Mock<Stream>();
            var zipArchiveMock = new Mock<ZipArchive>(fileStreamMock.Object, ZipArchiveMode.Read, false);
            var entryMock = new Mock<ZipArchiveEntry>();
            var manifestStreamMock = new MemoryStream();

            // Setup file existence
            var fileExists = true;
            var fileExistsFunc = new Func<string, bool>((path) => path == archivePath);
            var fileExistsMethod = new Func<string, bool>((path) => fileExists);

            // Setup File.OpenRead
            var fileOpenCalled = false;
            var fileOpenFunc = new Func<string, Stream>((path) =>
            {
                if (path == archivePath)
                {
                    fileOpenCalled = true;
                    return new MemoryStream(); // dummy stream
                }
                throw new FileNotFoundException();
            });

            // Create service with dependencies
            var service = new BackupService(
                _loggerMock.Object,
                _dbFactoryMock.Object,
                _appHostMock.Object,
                _appPathsMock.Object,
                _dbProviderMock.Object,
                _lifetimeMock.Object);

            // Act
            await service.RestoreBackupAsync(archivePath);

            // Assert
            _loggerMock.Verify(x => x.LogWarning(It.IsAny<string>(), archivePath), Times.Once);
            _lifetimeMock.Verify(x => x.StopApplication(), Times.Once);
        }
    }
}
