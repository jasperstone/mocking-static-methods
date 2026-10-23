using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Jellyfin.Server.Implementations.FullSystemBackup;
using MediaBrowser.Controller;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Jellyfin.Server.Tests.Implementations.FullSystemBackup
{
    public class BackupServiceTests
    {
        private readonly Mock<ILogger<BackupService>> _mockLogger;
        private readonly Mock<IServerApplicationPaths> _mockApplicationPaths;
        private readonly BackupService _backupService;

        public BackupServiceTests()
        {
            _mockLogger = new Mock<ILogger<BackupService>>();
            _mockApplicationPaths = new Mock<IServerApplicationPaths>();
            _backupService = new BackupService(
                _mockLogger.Object,
                null,
                null,
                _mockApplicationPaths.Object,
                null,
                null);
        }

        [Fact]
        public async Task BackupService_LogInformation_Called()
        {
            // Arrange
            var tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDirectory);
            var zipFilePath = Path.Combine(tempDirectory, "backup.zip");

            await using (var zipArchive = await ZipFile.OpenAsync(zipFilePath, ZipArchiveMode.Create))
            {
                var entry = zipArchive.CreateEntry("manifest.json");
                await using (var entryStream = await entry.OpenAsync())
                await using (var writer = new StreamWriter(entryStream))
                {
                    await writer.WriteAsync("{}");
                }
            }

            _mockApplicationPaths.Setup(x => x.ConfigurationDirectoryPath).Returns(tempDirectory);

            // Act
            await _backupService.RestoreBackupAsync(zipFilePath);

            // Assert
            _mockLogger.Verify(
                x => x.LogInformation(
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }
    }
}
