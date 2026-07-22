using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using Jellyfin.Server.Implementations.FullSystemBackup;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace Jellyfin.Tests
{
    public class BackupServiceTests
    {
        private readonly Mock<ILogger<BackupService>> _loggerMock;
        private readonly Mock<IDbContextFactory<JellyfinDbContext>> _dbFactoryMock;
        private readonly Mock<IServerApplicationHost> _appHostMock;
        private readonly Mock<IServerApplicationPaths> _pathsMock;
        private readonly Mock<IJellyfinDatabaseProvider> _dbProviderMock;
        private readonly Mock<IHostApplicationLifetime> _lifetimeMock;
        private readonly BackupService _backupService;

        public BackupServiceTests()
        {
            _loggerMock = new Mock<ILogger<BackupService>>();
            _dbFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            _appHostMock = new Mock<IServerApplicationHost>();
            _pathsMock = new Mock<IServerApplicationPaths>();
            _dbProviderMock = new Mock<IJellyfinDatabaseProvider>();
            _lifetimeMock = new Mock<IHostApplicationLifetime>();

            _backupService = new BackupService(
                _loggerMock.Object,
                _dbFactoryMock.Object,
                _appHostMock.Object,
                _pathsMock.Object,
                _dbProviderMock.Object,
                _lifetimeMock.Object);
        }

        [Fact]
        public async Task RestoreBackupAsync_LogsInformation_WhenLoggingCalled()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();
            var zipPath = Path.Combine(Path.GetTempPath(), "testbackup.zip");
            File.Copy(tempFile, zipPath, true);
            File.Delete(tempFile);

            // Create a minimal zip archive with manifest.json and a dummy database JSON
            using (var zipStream = new FileStream(zipPath, FileMode.Create))
            using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create))
            {
                var manifestEntry = archive.CreateEntry("manifest.json");
                using (var writer = new StreamWriter(manifestEntry.Open()))
                {
                    writer.Write("{\"ServerVersion\":\"1.0.0\",\"BackupEngineVersion\":\"0.2.0\"}");
                }

                var dbEntry = archive.CreateEntry("Database/HistoryRow.json");
                using (var writer = new StreamWriter(dbEntry.Open()))
                {
                    writer.Write("[]");
                }
            }

            // Setup mocks
            _pathsMock.Setup(p => p.ConfigurationDirectoryPath).Returns(Path.GetTempPath());
            _pathsMock.Setup(p => p.DataPath).Returns(Path.GetTempPath());
            _pathsMock.Setup(p => p.RootFolderPath).Returns(Path.GetTempPath());
            _pathsMock.Setup(p => p.InternalMetadataPath).Returns(Path.GetTempPath());
            _pathsMock.Setup(p => p.DefaultInternalMetadataPath).Returns(Path.GetTempPath());

            var mockDbContext = new Mock<JellyfinDbContext>();
            _dbFactoryMock.Setup(f => f.CreateDbContextAsync()).ReturnsAsync(mockDbContext.Object);

            // Act
            await _backupService.RestoreBackupAsync(zipPath);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Restore and override")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);

            // Cleanup
            File.Delete(zipPath);
        }
    }
}
