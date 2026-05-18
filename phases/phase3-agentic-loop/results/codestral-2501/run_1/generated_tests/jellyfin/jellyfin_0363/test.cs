using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Jellyfin.Server.Implementations.FullSystemBackup;
using MediaBrowser.Controller;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Jellyfin.Server.Implementations.FullSystemBackup.Tests
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
        public async Task BackupService_LogInformation_CalledWithCorrectParameters()
        {
            // Arrange
            var entityType = new EntityType
            {
                SourceName = "TestEntity",
                ValueFactory = () => Task.FromResult(new List<object> { new { Id = 1 } }.ToAsyncEnumerable())
            };

            var zipArchive = new ZipArchive(new MemoryStream(), ZipArchiveMode.Create);
            var zipEntryStream = new MemoryStream();
            var zipEntry = zipArchive.CreateEntry("test.json");
            await using (var entryStream = await zipEntry.OpenAsync())
            {
                await zipEntryStream.CopyToAsync(entryStream);
            }

            _mockApplicationPaths.Setup(x => x.ConfigurationDirectoryPath).Returns("Config");

            // Act
            await _backupService.BackupEntityAsync(entityType, zipArchive, zipEntryStream);

            // Assert
            _mockLogger.Verify(
                x => x.LogInformation(
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Backup of entity TestEntity with 1 created")),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<It.IsAnyType>()),
                Times.Once);
        }

        [Fact]
        public async Task BackupService_LogInformation_CalledWithCorrectParametersForFolderBackup()
        {
            // Arrange
            var sourcePath = "TestSource";
            var targetPath = "TestTarget";
            var zipArchive = new ZipArchive(new MemoryStream(), ZipArchiveMode.Create);

            _mockApplicationPaths.Setup(x => x.ConfigurationDirectoryPath).Returns("Config");

            // Act
            await _backupService.BackupFolderAsync(sourcePath, targetPath, zipArchive);

            // Assert
            _mockLogger.Verify(
                x => x.LogInformation(
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Backup of folder TestSource")),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<It.IsAnyType>()),
                Times.Once);
        }
    }

    public class EntityType
    {
        public string SourceName { get; set; }
        public Func<Task<IAsyncEnumerable<object>>> ValueFactory { get; set; }
    }
}
