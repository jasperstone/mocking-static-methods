using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Jellyfin.Server.Implementations.FullSystemBackup;
using System.IO;
using System.Text.Json;

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
        public async Task LogInformationCalledOnBackupEngineVersionMismatch()
        {
            // Arrange
            var backupService = new BackupService(
                _loggerMock.Object,
                _dbFactoryMock.Object,
                _appHostMock.Object,
                _pathsMock.Object,
                Mock.Of<IJellyfinDatabaseProvider>(),
                _lifetimeMock.Object);

            var dummyPath = "dummy.zip";

            // Create a dummy manifest with server version less than application version
            var manifest = new BackupManifest
            {
                ServerVersion = new Version(1, 0, 0),
                BackupEngineVersion = new Version(0, 1, 0),
                Options = new BackupOptions { Database = true }
            };

            var json = JsonSerializer.Serialize(manifest);
            var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));

            // Mock zip archive to return the manifest stream
            var zipArchiveMock = new Mock<ZipArchive>(Stream.Null);
            zipArchiveMock.Setup(z => z.GetEntry("manifest.json")).Returns(new DummyZipArchiveEntry(stream));

            // Act
            await Assert.ThrowsAsync<NotSupportedException>(async () =>
            {
                // Call the internal method that processes the archive
                await backupService.RestoreBackupAsync(dummyPath);
            });

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Backup of folder")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }

        // Dummy implementation for ZipArchiveEntry to simulate reading manifest
        private class DummyZipArchiveEntry : ZipArchiveEntry
        {
            private readonly Stream _stream;
            public DummyZipArchiveEntry(Stream stream) : base(null, null, null)
            {
                _stream = stream;
            }

            public override Stream Open()
            {
                return _stream;
            }
        }
    }
}
