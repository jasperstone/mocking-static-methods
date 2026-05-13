using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Jellyfin.Server.Implementations.FullSystemBackup;

namespace Jellyfin.Tests
{
    public class BackupServiceTests
    {
        private readonly Mock<ILogger<BackupService>> _loggerMock;
        private readonly Mock<IDbContextFactory<JellyfinDbContext>> _dbFactoryMock;
        private readonly Mock<IServerApplicationHost> _appHostMock;
        private readonly Mock<IServerApplicationPaths> _pathsMock;
        private readonly Mock<IHostApplicationLifetime> _lifetimeMock;

        public BackupServiceTests()
        {
            _loggerMock = new Mock<ILogger<BackupService>>();
            _dbFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            _appHostMock = new Mock<IServerApplicationHost>();
            _pathsMock = new Mock<IServerApplicationPaths>();
            _lifetimeMock = new Mock<IHostApplicationLifetime>();
        }

        [Fact]
        public async Task LogInformationCalledOnBackupServiceRestoreBackupAsync()
        {
            // Arrange
            var backupService = new BackupService(
                _loggerMock.Object,
                _dbFactoryMock.Object,
                _appHostMock.Object,
                _pathsMock.Object,
                Mock.Of<IJellyfinDatabaseProvider>(),
                _lifetimeMock.Object);

            var tempFile = Path.GetTempFileName();
            try
            {
                // Create a minimal valid zip archive with manifest.json
                using (var zipStream = new FileStream(tempFile, FileMode.Create))
                using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create))
                {
                    var manifestEntry = archive.CreateEntry("manifest.json");
                    using (var entryStream = manifestEntry.Open())
                    {
                        var manifest = new
                        {
                            ServerVersion = new Version(10, 0),
                            BackupEngineVersion = new Version(0, 2, 0),
                            Options = new { Database = false }
                        };
                        JsonSerializer.Serialize(entryStream, manifest);
                    }
                }

                // Act
                await backupService.RestoreBackupAsync(tempFile);

                // Assert
                _loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Information,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Restore and override")),
                        null,
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.AtLeastOnce);
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }
    }
}
